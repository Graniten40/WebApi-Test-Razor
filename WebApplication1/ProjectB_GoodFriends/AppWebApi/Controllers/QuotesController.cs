using System.Text.RegularExpressions;            
using Microsoft.AspNetCore.Authorization;        
using Microsoft.AspNetCore.Mvc;                   
using Microsoft.AspNetCore.Mvc.Filters;           

using Models.Interfaces;                          
using Models.DTO;                                
using Services.Interfaces;                        


namespace AppWebApi.Controllers
{
    // API-controller (model binding/conventions)
    [ApiController]

    // Route blir: /api/Quotes/<action>
    // Ex: /api/Quotes/Read, /api/Quotes/DeleteItem/{id}, etc.
    [Route("api/[controller]/[action]")]
    public class QuotesController : Controller
    {
        // DI-injicerad service + logger (sätts i konstruktorn)
        readonly IQuotesService _service = null;
        readonly ILogger<QuotesController> _logger = null;

        // GET: api/quotes/read
        [HttpGet()]
        [ActionName("Read")]

        // Swagger/dokumentation:
        // 200: ResponsePageDto<IQuote>
        // 400: string (felmeddelande)
        [ProducesResponseType(200, Type = typeof(ResponsePageDto<IQuote>))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> Read(string seeded = "true", string flat = "true",
            string filter = null, string pageNr = "0", string pageSize = "10")
        {
            try
            {
                // Query params kommer in som string och parsas manuellt här.
                // OBS: bool.Parse/int.Parse kastar exception vid ogiltiga värden.
                bool seededArg = bool.Parse(seeded);
                bool flatArg = bool.Parse(flat);
                int pageNrArg = int.Parse(pageNr);
                int pageSizeArg = int.Parse(pageSize);
                
                // RegEx-validering av filter:
                // Tillåter bara A–Z, a–z, 0–9 och whitespace.
                // Bra för att stoppa "konstiga" tecken, men blockerar t.ex. å/ä/ö, bindestreck, apostrof.
                if (!string.IsNullOrEmpty(filter) && !Regex.IsMatch(filter, @"^[a-zA-Z0-9\s]*$"))
                {
                    // Kastas => fångas i catch => returneras som 400
                    throw new ArgumentException("Filter can only contain letters (a-z), numbers (0-9), and spaces.");
                }
 
                // Loggar de tolkade argumenten
                _logger.LogInformation($"{nameof(Read)}: {nameof(seededArg)}: {seededArg}, {nameof(flatArg)}: {flatArg}, " +
                    $"{nameof(pageNrArg)}: {pageNrArg}, {nameof(pageSizeArg)}: {pageSizeArg}");

                // Kallar service:
                // - filter trimmas + ToLower() (current culture)
                // - filter kan fortfarande vara null
                var resp = await _service.ReadQuotesAsync(seededArg, flatArg, filter?.Trim().ToLower(), pageNrArg, pageSizeArg);     
                return Ok(resp);
            }
            catch (Exception ex)
            {
                // Catch-all => alla fel blir 400 BadRequest (även "not found" om du kastar)
                _logger.LogError($"{nameof(Read)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        // GET: api/quotes/readitem
        [HttpGet()]
        [ActionName("ReadItem")]

        // Swagger säger att 404 kan förekomma,
        // men i praktiken returnerar du aldrig NotFound() här – du returnerar BadRequest i catch.
        [ProducesResponseType(200, Type = typeof(IQuote))]
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

                // OBS: här skickar du alltid "false" till service,
                // så parametern flatArg används inte i serviceanropet.
                // (Det är inte “fel” i sig, men bra att känna till: queryparametern flat påverkar inte.)
                var item = await _service.ReadQuoteAsync(idArg, false);

                // Om saknas: du kastar ArgumentException -> hamnar i catch -> 400
                if (item == null) throw new ArgumentException ($"Item with id {id} does not exist");

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ReadItem)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/quotes/deleteitem/{id}
        // OBS: eftersom controller-route är api/[controller]/[action],
        // så blir route här: api/Quotes/DeleteItem/{id}
        [HttpDelete("{id}")]
        [ActionName("DeleteItem")]
        [ProducesResponseType(200, Type = typeof(IQuote))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> DeleteItem(string id)
        {   
            try
            {
                var idArg = Guid.Parse(id);
                
                _logger.LogInformation($"{nameof(DeleteItem)}: {nameof(idArg)}: {idArg}");

                // Service raderar och returnerar den borttagna (eller null om saknas)
                var item = await _service.DeleteQuoteAsync(idArg);

                // Saknas -> ArgumentException -> catch -> 400
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

        // GET: api/quotes/readitemdto
        [HttpGet()]
        [ActionName("ReadItemDto")]

        // OBS: Attributet säger 200 Type = QuoteCuDto,
        // men du returnerar ResponseItemDto<QuoteCuDto>.
        // Swagger kan alltså visa fel responsmodell om den inte tolkar generics/wrappern rätt.
        [ProducesResponseType(200, Type = typeof(QuoteCuDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        public async Task<IActionResult> ReadItemDto(string id = null)
        {
            try
            {
                var idArg = Guid.Parse(id);

                _logger.LogInformation($"{nameof(ReadItemDto)}: {nameof(idArg)}: {idArg}");

                // Läser quote med flat=false
                var item = await _service.ReadQuoteAsync(idArg, false);

                // Saknas -> ArgumentException -> catch -> 400
                if (item == null) throw new ArgumentException($"Item with id {id} does not exist");

                return Ok(
                    // Wrapper-response: innehåller Item + ev ConnectionString i DEBUG
                    new ResponseItemDto<QuoteCuDto>() {
#if DEBUG
                    // Bara i DEBUG: hjälper vid felsökning, men ska inte läcka i prod
                    ConnectionString = item.ConnectionString,
#endif
                    // QuoteCuDto byggs från domänobjektet
                    Item = new QuoteCuDto(item.Item)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ReadItemDto)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/quotes/updateitem/{id}
        [HttpPut("{id}")]
        [ActionName("UpdateItem")]
        [ProducesResponseType(200, Type = typeof(IQuote))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> UpdateItem(string id, [FromBody] QuoteCuDto item)
        {
            try
            {
                // DTO-validering (troligen kastar exception om något är ogiltigt)
                item.EnsureValidity();

                var idArg = Guid.Parse(id);
                _logger.LogInformation($"{nameof(UpdateItem)}: {nameof(idArg)}: {idArg}");

                // Säkrar att route-id matchar body-id
                if (item.QuoteId != idArg) throw new ArgumentException("Id mismatch");

                // Uppdaterar i service
                var model = await _service.UpdateQuoteAsync(item);
                _logger.LogInformation($"item {idArg} updated");
               
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(UpdateItem)}: {ex.Message}");
                return BadRequest($"Could not update. Error {ex.Message}");
            }
        }

        // POST: api/quotes/createitem
        [HttpPost()]
        [ActionName("CreateItem")]
        [ProducesResponseType(200, Type = typeof(IQuote))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> CreateItem([FromBody] QuoteCuDto item)
        {
            try
            {
                // DTO-validering
                item.EnsureValidity();

                // Loggar att create startar
               _logger.LogInformation($"{nameof(CreateItem)}:");
               
                // Skapar i service
                var model = await _service.CreateQuoteAsync(item);

                // Loggar nytt quote-id
                _logger.LogInformation($"item {model.Item.QuoteId} created");

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CreateItem)}: {ex.Message}");
                return BadRequest($"Could not create. Error {ex.Message}");
            }
        }

        // Konstruktorn: DI fyller i service + logger
        public QuotesController(IQuotesService service, ILogger<QuotesController> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}
