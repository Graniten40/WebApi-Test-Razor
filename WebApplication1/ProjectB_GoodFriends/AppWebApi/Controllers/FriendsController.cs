using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using DbContext;                 // MainDbContext
using Models.Interfaces;
using Models.DTO;
using Services.Interfaces;
using System.Text.RegularExpressions;

namespace AppWebApi.Controllers
{
    /// <summary>
    /// Friends controller exposing endpoints for:
    /// - paged reads (service-based)
    /// - single item reads/deletes (service-based)
    /// - DTO reads for client editing
    /// - update with server-side validation + safe Address update (DB-based)
    /// - list by location (DB-based)
    /// 
    /// Routing uses "api/[controller]/[action]" => e.g. api/Friends/Read, api/Friends/UpdateItem/{id}, etc.
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FriendsController : Controller
    {
        // Service layer handles existing CRUD methods and data-access logic (for consistency with the rest of the API).
        private readonly IFriendsService _service;

        // Direct DB context is injected because some actions (List + UpdateItem) are implemented with EF queries/updates.
        private readonly MainDbContext _db;

        // Logging for debugging, traceability and server diagnostics.
        private readonly ILogger<FriendsController> _logger;

        /// <summary>
        /// Controller constructor: injects service, DB context and logger.
        /// (DB is used for the new location listing + the custom update logic.)
        /// </summary>
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

        /// <summary>
        /// Lists friends filtered by Country and/or City.
        /// Uses EF Core directly and AsNoTracking for a read-only query.
        /// Input is validated via a safe regex pattern allowing letters (Unicode), digits, spaces, hyphen and apostrophe.
        /// </summary>
        [HttpGet]
        [ActionName("List")]
        [ProducesResponseType(200, Type = typeof(List<FriendListLocationDto>))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> List(string? country = null, string? city = null)
        {
            try
            {
                // Accept Unicode letters (\p{L}) so countries/cities like "Gävle" work.
                // Also allows digits, whitespace, hyphen, apostrophe.
                const string safePattern = @"^[\p{L}0-9\s\-']*$";

                // Basic hardening: reject unexpected characters early
                if (!string.IsNullOrWhiteSpace(country) && !Regex.IsMatch(country, safePattern))
                    return BadRequest("Country contains invalid characters.");

                if (!string.IsNullOrWhiteSpace(city) && !Regex.IsMatch(city, safePattern))
                    return BadRequest("City contains invalid characters.");

                // Normalize incoming parameters
                var countryArg = country?.Trim();
                var cityArg = city?.Trim();

                _logger.LogInformation($"{nameof(List)}: country={countryArg}, city={cityArg}");

                // Start query: only friends that have an Address
                // AsNoTracking improves performance for read-only lists.
                var q = _db.Friends
                    .AsNoTracking()
                    .Where(f => f.AddressDbM != null);

                // Apply filters only if present
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

                // Order results to get stable output in UI
                // Project to lightweight DTO to avoid sending full entity graph
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
                // In case of unexpected DB/EF issues, return a BadRequest with message (simple error model for this project)
                _logger.LogError(ex, $"{nameof(List)} failed");
                return BadRequest(ex.Message);
            }
        }

        // -----------------------------
        // Existing actions 
        // -----------------------------

        /// <summary>
        /// Paged read list of friends (service-based).
        /// Parses querystring params and validates filter (letters/digits/spaces only).
        /// </summary>
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

                // Ensure filter only contains safe characters (matches client-side sanitize used in Razor app)
                if (!string.IsNullOrEmpty(filter) && !Regex.IsMatch(filter, @"^[a-zA-Z0-9\s]*$"))
                {
                    throw new ArgumentException("Filter can only contain letters (a-z), numbers (0-9), and spaces.");
                }

                _logger.LogInformation($"{nameof(Read)}: {nameof(seededArg)}: {seededArg}, {nameof(flatArg)}: {flatArg}, " +
                    $"{nameof(pageNrArg)}: {pageNrArg}, {nameof(pageSizeArg)}: {pageSizeArg}");

                // Delegate query to service layer
                var resp = await _service.ReadFriendsAsync(seededArg, flatArg, filter?.Trim().ToLower(), pageNrArg, pageSizeArg);
                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(Read)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Read a single friend by id (service-based).
        /// flat controls whether to include deeper relations.
        /// </summary>
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

        /// <summary>
        /// Deletes a friend by id (service-based).
        /// </summary>
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

        /// <summary>
        /// Reads a single friend but returns a Create/Update DTO wrapper.
        /// Used by clients when editing a friend (simpler payload than full domain model).
        /// </summary>
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

                // Wrap the DTO inside ResponseItemDto (can optionally include connection string in DEBUG)
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

        /// <summary>
        /// Update endpoint for Friend including optional Address update.
        /// Uses EF Core directly to ensure:
        /// - server-side ModelState validation (DataAnnotations)
        /// - uniqueness check for Email
        /// - safe create/update of Address navigation
        /// Returns:
        /// - 400 for invalid id
        /// - 400 with ValidationProblem for model errors
        /// - 404 if Friend doesn't exist
        /// - 204 NoContent on success (common REST pattern for update)
        /// </summary>
        [HttpPut("{id}")]
        [ActionName("UpdateItem")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateItem(string id, [FromBody] FriendUpdateRequestDto dto)
        {
            // 1) Validate and parse route id
            if (!Guid.TryParse(id, out var idArg))
                return BadRequest("Invalid id.");

            // 2) Validate DTO (DataAnnotations => ModelState)
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            // 3) Load existing Friend + Address from DB (Address included so it can be updated safely)
            var friendDb = await _db.Friends
                .Include(f => f.AddressDbM)
                .FirstOrDefaultAsync(f => f.FriendId == idArg);

            if (friendDb == null)
                return NotFound($"Item with id {idArg} does not exist");

            // 4) Server-side uniqueness check for Email (avoid duplicates)
            var emailTaken = await _db.Friends.AnyAsync(f => f.FriendId != idArg && f.Email == dto.Email);
            if (emailTaken)
            {
                // Add error to ModelState so client can show it under the Email field
                ModelState.AddModelError(nameof(dto.Email), "Email is already in use.");
                return ValidationProblem(ModelState);
            }

            // 5) Update Friend fields
            friendDb.FirstName = dto.FirstName;
            friendDb.LastName  = dto.LastName;
            friendDb.Email     = dto.Email;
            friendDb.Birthday  = dto.Birthday;

            // 6) Update Address only if provided (Address is optional)
            //    If dto.Address is null, existing address is left unchanged.
            if (dto.Address != null)
            {
                // If friend doesn't have an address yet, create one
                if (friendDb.AddressDbM == null)
                {
                    friendDb.AddressDbM = new DbModels.AddressDbM();
                }

                // Extra server-side validation for ZipCode (if not fully covered by DataAnnotations)
                if (dto.Address.ZipCode < 0 || dto.Address.ZipCode > 99999)
                {
                    ModelState.AddModelError("Address.ZipCode", "ZipCode must be between 0 and 99999.");
                    return ValidationProblem(ModelState);
                }

                // Normalize strings (trim) and avoid nulls
                friendDb.AddressDbM.StreetAddress = dto.Address.StreetAddress?.Trim() ?? "";
                friendDb.AddressDbM.ZipCode       = dto.Address.ZipCode;
                friendDb.AddressDbM.City          = dto.Address.City?.Trim() ?? "";
                friendDb.AddressDbM.Country       = dto.Address.Country?.Trim() ?? "";
            }

            // 7) Persist changes
            await _db.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Creates a friend (service-based).
        /// DTO validates itself via EnsureValidity() before calling service.
        /// </summary>
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
