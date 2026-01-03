using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using DbContext;                 // MainDbContext
using Models.Interfaces;
using Models.DTO;
using Services.Interfaces;
using System.Text.RegularExpressions;

namespace AppWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FriendsController : Controller
    {
        private readonly IFriendsService _service;
        private readonly MainDbContext _db;
        private readonly ILogger<FriendsController> _logger;

        // Uppdaterad ctor: vi injicerar både service + db + logger
        public FriendsController(IFriendsService service, MainDbContext db, ILogger<FriendsController> logger)
        {
            _service = service;
            _db = db;
            _logger = logger;
        }

        // -----------------------------
        // NEW: List friends by Country and/or City (DB-based)
        // GET: api/Friends/List?country=Sweden&city=Uppsala
        // -----------------------------
        [HttpGet]
        [ActionName("List")]
        [ProducesResponseType(200, Type = typeof(List<FriendListLocationDto>))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> List(string? country = null, string? city = null)
        {
            try
            {
                const string safePattern = @"^[\p{L}0-9\s\-']*$";

                if (!string.IsNullOrWhiteSpace(country) && !Regex.IsMatch(country, safePattern))
                    return BadRequest("Country contains invalid characters.");

                if (!string.IsNullOrWhiteSpace(city) && !Regex.IsMatch(city, safePattern))
                    return BadRequest("City contains invalid characters.");

                var countryArg = country?.Trim();
                var cityArg = city?.Trim();

                _logger.LogInformation($"{nameof(List)}: country={countryArg}, city={cityArg}");

                var q = _db.Friends
                    .AsNoTracking()
                    .Where(f => f.AddressDbM != null);

                if (!string.IsNullOrWhiteSpace(countryArg))
                {
                    q = q.Where(f => !string.IsNullOrWhiteSpace(f.AddressDbM!.Country) &&
                                     f.AddressDbM.Country.Trim() == countryArg);
                }

                if (!string.IsNullOrWhiteSpace(cityArg))
                {
                    q = q.Where(f => !string.IsNullOrWhiteSpace(f.AddressDbM!.City) &&
                                     f.AddressDbM.City.Trim() == cityArg);
                }

                var result = await q
                    .OrderBy(f => f.AddressDbM!.Country)
                    .ThenBy(f => f.AddressDbM!.City)
                    .ThenBy(f => f.FirstName)
                    .ThenBy(f => f.LastName)
                    .Select(f => new FriendListLocationDto
                    {
                        FriendId = f.FriendId,        
                        FirstName = f.FirstName,
                        LastName = f.LastName,
                        Email = f.Email,
                        Country = f.AddressDbM!.Country,
                        City = f.AddressDbM!.City
                    })
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(List)} failed");
                return BadRequest(ex.Message);
            }
        }

        // -----------------------------
        // Existing actions 
        // -----------------------------

        [HttpGet()]
        [ActionName("Read")]
        [ProducesResponseType(200, Type = typeof(ResponsePageDto<IFriend>))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> Read(string seeded = "true", string flat = "true",
            string filter = null, string pageNr = "0", string pageSize = "10")
        {
            try
            {
                bool seededArg = bool.Parse(seeded);
                bool flatArg = bool.Parse(flat);
                int pageNrArg = int.Parse(pageNr);
                int pageSizeArg = int.Parse(pageSize);

                // RegEx check to ensure filter only contains a-z, 0-9, and spaces
                if (!string.IsNullOrEmpty(filter) && !Regex.IsMatch(filter, @"^[a-zA-Z0-9\s]*$"))
                {
                    throw new ArgumentException("Filter can only contain letters (a-z), numbers (0-9), and spaces.");
                }

                _logger.LogInformation($"{nameof(Read)}: {nameof(seededArg)}: {seededArg}, {nameof(flatArg)}: {flatArg}, " +
                    $"{nameof(pageNrArg)}: {pageNrArg}, {nameof(pageSizeArg)}: {pageSizeArg}");

                var resp = await _service.ReadFriendsAsync(seededArg, flatArg, filter?.Trim().ToLower(), pageNrArg, pageSizeArg);
                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(Read)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet()]
        [ActionName("ReadItem")]
        [ProducesResponseType(200, Type = typeof(IFriend))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        public async Task<IActionResult> ReadItem(string id = null, string flat = "false")
        {
            try
            {
                var idArg = Guid.Parse(id);
                bool flatArg = bool.Parse(flat);

                _logger.LogInformation($"{nameof(ReadItem)}: {nameof(idArg)}: {idArg}, {nameof(flatArg)}: {flatArg}");

                var item = await _service.ReadFriendAsync(idArg, flatArg);
                if (item == null) throw new ArgumentException($"Item with id {id} does not exist");

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ReadItem)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ActionName("DeleteItem")]
        [ProducesResponseType(200, Type = typeof(IFriend))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> DeleteItem(string id)
        {
            try
            {
                var idArg = Guid.Parse(id);

                _logger.LogInformation($"{nameof(DeleteItem)}: {nameof(idArg)}: {idArg}");

                var item = await _service.DeleteFriendAsync(idArg);
                if (item == null) throw new ArgumentException($"Item with id {id} does not exist");

                _logger.LogInformation($"item {idArg} deleted");
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(DeleteItem)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet()]
        [ActionName("ReadItemDto")]
        [ProducesResponseType(200, Type = typeof(FriendCuDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        public async Task<IActionResult> ReadItemDto(string id = null)
        {
            try
            {
                var idArg = Guid.Parse(id);

                _logger.LogInformation($"{nameof(ReadItemDto)}: {nameof(idArg)}: {idArg}");

                var item = await _service.ReadFriendAsync(idArg, false);
                if (item == null) throw new ArgumentException($"Item with id {id} does not exist");

                return Ok(
                    new ResponseItemDto<FriendCuDto>()
                    {
#if DEBUG
                        ConnectionString = item.ConnectionString,
#endif
                        Item = new FriendCuDto(item.Item)
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ReadItemDto)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ActionName("UpdateItem")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateItem(string id, [FromBody] FriendUpdateRequestDto dto)
        {
            // 1) Guid-parse
            if (!Guid.TryParse(id, out var idArg))
                return BadRequest("Invalid id.");

            // 2) DataAnnotations => ModelState
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            // 3) Hämta befintlig Friend + Address från DB (vi ska uppdatera address säkert)
            var friendDb = await _db.Friends
                .Include(f => f.AddressDbM)
                .FirstOrDefaultAsync(f => f.FriendId == idArg);

            if (friendDb == null)
                return NotFound($"Item with id {idArg} does not exist");

            // 4) Unik email server-side
            var emailTaken = await _db.Friends.AnyAsync(f => f.FriendId != idArg && f.Email == dto.Email);
            if (emailTaken)
            {
                ModelState.AddModelError(nameof(dto.Email), "Email is already in use.");
                return ValidationProblem(ModelState);
            }

            // 5) Uppdatera Friend-fälten (DB)
            friendDb.FirstName = dto.FirstName;
            friendDb.LastName  = dto.LastName;
            friendDb.Email     = dto.Email;
            friendDb.Birthday  = dto.Birthday;

            // 6) Uppdatera Address (om dto har address)
            //    - Tillåter null (då lämnar vi befintlig address orörd)
            if (dto.Address != null)
            {
                // Om friend saknar address -> skapa
                if (friendDb.AddressDbM == null)
                {
                    friendDb.AddressDbM = new DbModels.AddressDbM(); 
                }

                // (Valfritt men bra) server-side check: ZipCode rimligt (om du inte redan har DataAnnotations)
                // Ex: om ZipCode ska vara 0..99999
                if (dto.Address.ZipCode < 0 || dto.Address.ZipCode > 99999)
                {
                    ModelState.AddModelError("Address.ZipCode", "ZipCode must be between 0 and 99999.");
                    return ValidationProblem(ModelState);
                }

                friendDb.AddressDbM.StreetAddress = dto.Address.StreetAddress?.Trim() ?? "";
                friendDb.AddressDbM.ZipCode       = dto.Address.ZipCode;
                friendDb.AddressDbM.City          = dto.Address.City?.Trim() ?? "";
                friendDb.AddressDbM.Country       = dto.Address.Country?.Trim() ?? "";
            }

            // 7) Spara i DB
            await _db.SaveChangesAsync();

            return NoContent();
        }



        [HttpPost()]
        [ActionName("CreateItem")]
        [ProducesResponseType(200, Type = typeof(IFriend))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> CreateItem([FromBody] FriendCuDto item)
        {
            try
            {
                item.EnsureValidity();
                _logger.LogInformation($"{nameof(CreateItem)}:");

                var _item = await _service.CreateFriendAsync(item);
                _logger.LogInformation($"item {_item.Item.FriendId} created");

                return Ok(_item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CreateItem)}: {ex.Message}");
                return BadRequest($"Could not create. Error {ex.Message}");
            }
        }
    }
}
