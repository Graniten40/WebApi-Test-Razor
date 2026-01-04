using System.Text.RegularExpressions;             
using Microsoft.AspNetCore.Authorization;        
using Microsoft.AspNetCore.Mvc;                   
using Microsoft.AspNetCore.Mvc.Filters;           

using Models.Interfaces;                          
using Models.DTO;                                 
using Services.Interfaces;                        


namespace AppWebApi.Controllers
{
    // Gör detta till en API-controller:
    // - bättre binding
    // - automatisk 400 på invalid ModelState (om du använder model binding + validation attributes)
    [ApiController]

    // Route blir: api/Pets/<action>
    // Ex: api/Pets/Read, api/Pets/CreateItem, etc.
    [Route("api/[controller]/[action]")]
    public class PetsController : Controller
    {
        // Services via DI.
        // OBS: Du sätter "= null" men de fylls i av konstruktorn.
        // "readonly" gör att de inte kan bytas efter konstruktion (bra).
        readonly IPetsService _service = null;
        readonly ILogger<PetsController> _logger = null;


        //GET: api/pets/read
        [HttpGet()]
        [ActionName("Read")]

        // Dokumenterar swagger/responses:
        // 200: en ResponsePageDto<IPet>
        // 400: string (felmeddelande)
        [ProducesResponseType(200, Type = typeof(ResponsePageDto<IPet>))]
        [ProducesResponseType(400, Type = typeof(string))]

        // Query params kommer som string default:
        // seeded, flat, pageNr, pageSize -> string som du själv parsar
        // filter = null default (OBS: det är en nullable-param men typen är "string" utan ?)
        public async Task<IActionResult> Read(string seeded = "true", string flat = "true",
            string filter = null, string pageNr = "0", string pageSize = "10")
        {
            try
            {
                // Parsar query-strängarna till rätt typer.
                // OBS: bool.Parse/int.Parse kastar Exception om värdet är ogiltigt (t.ex. seeded=maybe).
                bool seededArg = bool.Parse(seeded);
                bool flatArg = bool.Parse(flat);
                int pageNrArg = int.Parse(pageNr);
                int pageSizeArg = int.Parse(pageSize);

                // RegEx: tillåter bara a-z/A-Z, 0-9 och whitespace i filter.
                // Bra för att stoppa "konstiga" tecken i filter.
                // OBS: om du vill tillåta t.ex. åäö, bindestreck eller apostrof så blockeras de här.
                if (!string.IsNullOrEmpty(filter) && !Regex.IsMatch(filter, @"^[a-zA-Z0-9\s]*$"))
                {
                    // Du kastar ArgumentException -> fångas i catch och blir 400
                    throw new ArgumentException("Filter can only contain letters (a-z), numbers (0-9), and spaces.");
                }
 
                // Loggar vilka argument som faktiskt används
                 _logger.LogInformation($"{nameof(Read)}: {nameof(seededArg)}: {seededArg}, {nameof(flatArg)}: {flatArg}, " +
                    $"{nameof(pageNrArg)}: {pageNrArg}, {nameof(pageSizeArg)}: {pageSizeArg}");
                
                // Kallar service med:
                // - filter trimmas och görs lower-case (kultur: ToLower() använder current culture)
                // - null-säkert via filter?.Trim().ToLower()
                // OBS: i service måste du hantera null filter.
                var resp = await _service.ReadPetsAsync(seededArg, flatArg, filter?.Trim().ToLower(), pageNrArg, pageSizeArg);     
                return Ok(resp);
            }
            catch (Exception ex)
            {
                // Catch-all -> alla fel blir 400 BadRequest
                // OBS: även "not found" och "format errors" blir 400 här.
                _logger.LogError($"{nameof(Read)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        // GET api/pets/readitem
        [HttpGet()]
        [ActionName("Readitem")]

        // 200: IPet
        // 400: string
        // 404: string (men OBS: du returnerar aldrig NotFound(), du returnerar BadRequest i catch)
        [ProducesResponseType(200, Type = typeof(IPet))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        public async Task<IActionResult> ReadItem(string id = null, string flat = "false")
        {
            try
            {
                // Guid.Parse kastar exception om id är null eller fel format
                var idArg = Guid.Parse(id);

                // bool.Parse kastar exception om flat inte är "true"/"false"
                bool flatArg = bool.Parse(flat);

                _logger.LogInformation($"{nameof(ReadItem)}: {nameof(idArg)}: {idArg}, {nameof(flatArg)}: {flatArg}");
                
                var item = await _service.ReadPetAsync(idArg, flatArg);

                // Här kastar du ArgumentException om item saknas.
                // OBS: I catch blir detta 400 BadRequest, inte 404.
                if (item == null) throw new ArgumentException ($"Item with id {id} does not exist");

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ReadItem)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/pets/{id}
        // OBS: Route här använder {id} i attributet, men controller route är api/[controller]/[action].
        // Det betyder att slutlig route blir: api/Pets/DeleteItem/{id}
        // (inte bara api/pets/{id}, eftersom [action] ingår i controller-route)
        [HttpDelete("{id}")]

        // 200: IPet (den borttagna)
        // 400: string
        [ProducesResponseType(200, Type = typeof(IPet))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> DeleteItem(string id)
        {
            try
            {
                var idArg = Guid.Parse(id);

                _logger.LogInformation($"{nameof(DeleteItem)}: {nameof(idArg)}: {idArg}");
                
                var item = await _service.DeletePetAsync(idArg);

                // Samma sak här: saknas => ArgumentException => 400 (inte 404)
                if (item == null) throw new ArgumentException ($"Item with id {id} does not exist");
        
                _logger.LogInformation($"item {idArg} deleted");
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(DeleteItem)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        // GET api/pets/readitemdto
        // Returnerar en wrapper: ResponseItemDto<PetCuDto>
        [HttpGet()]
        [ActionName("ReadItemDto")]

        // OBS: Attributet säger 200 Type = PetCuDto,
        // men du returnerar ResponseItemDto<PetCuDto>.
        // Swagger-dokumentationen kan bli missvisande här.
        [ProducesResponseType(200, Type = typeof(PetCuDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        public async Task<IActionResult> ReadItemDto(string id = null)
        {
            try
            {
                var idArg = Guid.Parse(id);

                _logger.LogInformation($"{nameof(ReadItemDto)}: {nameof(idArg)}: {idArg}");

                // Läser pet "flat=false" (dvs du vill ha relations enligt service-logik)
                var item = await _service.ReadPetAsync(idArg, false);

                // Saknas => ArgumentException => 400 i catch
                if (item == null) throw new ArgumentException($"Item with id {id} does not exist");

                return Ok(
                    // Wrapper som innehåller Item + ev connection string i DEBUG
                    new ResponseItemDto<PetCuDto>() {
#if DEBUG
                    // Bara i DEBUG-kompilering skickar du med ConnectionString.
                    // OBS: Bra för felsökning, men viktigt att inte läcka i production.
                    ConnectionString = item.ConnectionString,
#endif
                    // PetCuDto byggs från item.Item (din domänmodell)
                    Item = new PetCuDto(item.Item)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ReadItemDto)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        // PUT api/pets/{id}  (i praktiken: api/Pets/UpdateItem/{id})
        [HttpPut("{id}")]
        [ActionName("UpdateItem")]
        [ProducesResponseType(200, Type = typeof(IPet))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> UpdateItem(string id, [FromBody] PetCuDto item)
        {
            try
            {
                // Validering som du har definierat i DTO:n (troligen kastar exceptions om invalid)
                item.EnsureValidity();

                var idArg = Guid.Parse(id);
                _logger.LogInformation($"{nameof(UpdateItem)}: {nameof(idArg)}: {idArg}");

                // Säkerställer att route-id matchar body-id
                if (item.PetId != idArg) throw new ArgumentException("Id mismatch");

                // Service uppdaterar
                var model = await _service.UpdatePetAsync(item);
                _logger.LogInformation($"item {idArg} updated");
               
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(UpdateItem)}: {ex.Message}");

                // Returnerar 400 med ett lite mer “friendly” felmeddelande
                return BadRequest($"Could not update. Error {ex.Message}");
            }
        }

        // POST api/pets/createitem
        [HttpPost()]
        [ActionName("CreateItem")]
        [ProducesResponseType(200, Type = typeof(IPet))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> CreateItem([FromBody] PetCuDto item)
        {
            try
            {
                // DTO-validering
                item.EnsureValidity();

                // Loggar att create startar (utan payload)
                _logger.LogInformation($"{nameof(CreateItem)}:");
                
                // Skapar via service
                var model = await _service.CreatePetAsync(item);

                // Loggar nya id:t
                _logger.LogInformation($"item {model.Item.PetId} created");

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CreateItem)}: {ex.Message}");
                return BadRequest($"Could not create. Error {ex.Message}");
            }
        }

        // Konstruktor för DI: här fylls _service och _logger
        public PetsController(IPetsService service, ILogger<PetsController> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}
