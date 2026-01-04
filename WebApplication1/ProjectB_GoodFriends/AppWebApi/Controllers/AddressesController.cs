using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

using Models.Interfaces;
using Models.DTO;
using Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppWebApi.Controllers
{
    /// <summary>
    /// Controller for CRUD operations on Address resources.
    /// Uses "api/[controller]/[action]" routing, so endpoints become:
    /// - api/Addresses/Read
    /// - api/Addresses/ReadItem
    /// - api/Addresses/DeleteItem/{id}
    /// - api/Addresses/ReadItemDto
    /// - api/Addresses/UpdateItem/{id}
    /// - api/Addresses/CreateItem
    /// 
    /// The controller is thin: it validates/parses input, logs, and delegates work to the service layer.
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AddressesController : Controller
    {
        // Service layer encapsulates business logic + data access
        readonly IAddressesService _service;

        // Logger used for traceability and debugging
        readonly ILogger<AddressesController> _logger;

        /// <summary>
        /// READ (paged list).
        /// Querystring parameters are received as strings and parsed to typed args to be robust against missing values.
        /// Supports:
        /// - seeded: include seeded/test data
        /// - flat: whether to return a "flat" projection or include relations
        /// - filter: free text filter (validated via regex)
        /// - pageNr, pageSize: pagination
        /// </summary>
        [HttpGet()]
        [ActionName("Read")]
        [ProducesResponseType(200, Type = typeof(ResponsePageDto<IAddress>))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> Read(string seeded = "true", string flat = "true",
            string filter = null, string pageNr = "0", string pageSize = "10")
        {
            try
            {
                // Parse querystring values to typed arguments
                bool seededArg = bool.Parse(seeded);
                bool flatArg = bool.Parse(flat);
                int pageNrArg = int.Parse(pageNr);
                int pageSizeArg = int.Parse(pageSize);
     
                // Input validation: allow only letters, digits, and spaces in filter
                // (reduces risk of malformed queries / unexpected characters)
                if (!string.IsNullOrEmpty(filter) && !Regex.IsMatch(filter, @"^[a-zA-Z0-9\s]*$"))
                {
                    throw new ArgumentException("Filter can only contain letters (a-z), numbers (0-9), and spaces.");
                }
     
                // Log parameters for traceability (useful when debugging paging/filter issues)
                _logger.LogInformation($"{nameof(Read)}: {nameof(seededArg)}: {seededArg}, {nameof(flatArg)}: {flatArg}, " +
                    $"{nameof(pageNrArg)}: {pageNrArg}, {nameof(pageSizeArg)}: {pageSizeArg}");
                
                // Delegate to service layer; normalize filter (trim + lower) before passing along
                var resp = await _service.ReadAddressesAsync(seededArg, flatArg, filter?.Trim().ToLower(), pageNrArg, pageSizeArg);     
                return Ok(resp);
            }
            catch (Exception ex)
            {
               // On any exception, log and return a 400 with message (simple error model for this project)
               _logger.LogError($"{nameof(Read)}: {ex.Message}");
                 return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// READ single item by id.
        /// id is received as string (route/query) and parsed to Guid.
        /// flat controls whether to include deeper relations.
        /// </summary>
        [HttpGet()]
        [ActionName("ReadItem")]
        [ProducesResponseType(200, Type = typeof(IAddress))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        public async Task<IActionResult> ReadItem(string id = null, string flat = "false")
        {
            try
            {
                // Parse id + flags
                var idArg = Guid.Parse(id);
                bool flatArg = bool.Parse(flat);

                _logger.LogInformation($"{nameof(ReadItem)}: {nameof(idArg)}: {idArg}, {nameof(flatArg)}: {flatArg}");
                
                // Service returns null if not found
                var item = await _service.ReadAddressAsync(idArg, flatArg);

                // Here a missing item is treated as an argument error (returned as 400)
                // (Could also be NotFound, but this project uses BadRequest for simplicity)
                if (item == null) throw new ArgumentException ($"Item with id {id} does not exist");
                
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ReadItem)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// DELETE item by id (id is part of route).
        /// </summary>
        [HttpDelete("{id}")]
        [ActionName("DeleteItem")]
        [ProducesResponseType(200, Type = typeof(IAddress))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> DeleteItem(string id)
        {
            try
            {
                var idArg = Guid.Parse(id);

                _logger.LogInformation($"{nameof(DeleteItem)}: {nameof(idArg)}: {idArg}");

                // Service deletes and returns deleted entity, or null if not found
                var item = await _service.DeleteAddressAsync(idArg);
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

        /// <summary>
        /// READ single item but wrapped in a DTO response format (ResponseItemDto<AddressCuDto>).
        /// Intended for client editing (CU = Create/Update DTO).
        /// </summary>
        [HttpGet()]
        [ActionName("ReadItemDto")]
        [ProducesResponseType(200, Type = typeof(AddressCuDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        public async Task<IActionResult> ReadItemDto(string id = null)
        {
            try
            {
                var idArg = Guid.Parse(id);

                _logger.LogInformation($"{nameof(ReadItemDto)}: {nameof(idArg)}: {idArg}");

                // Read without "flat" relations for edit payloads
                var item = await _service.ReadAddressAsync(idArg, false);
                if (item == null) throw new ArgumentException($"Item with id {id} does not exist");

                // Wrap into ResponseItemDto; include ConnectionString only in DEBUG builds
                return Ok(
                    new ResponseItemDto<AddressCuDto>() {
#if DEBUG
                    ConnectionString = item.ConnectionString,
#endif
                    Item = new AddressCuDto(item.Item)
                });

            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ReadItemDto)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// UPDATE item by id (id in route, body contains AddressCuDto).
        /// Validates DTO first, checks id mismatch, then delegates to service.
        /// </summary>
        [HttpPut("{id}")]
        [ActionName("UpdateItem")]
        [ProducesResponseType(200, Type = typeof(IAddress))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> UpdateItem(string id, [FromBody] AddressCuDto item)
        {
            try
            {
                // Domain/DTO validation handled by DTO itself
                item.EnsureValidity();

                var idArg = Guid.Parse(id);
                _logger.LogInformation($"{nameof(UpdateItem)}: {nameof(idArg)}: {idArg}");

                // Prevent updating the wrong entity (route id must match body id)
                if (item.AddressId != idArg) throw new ArgumentException("Id mismatch");

                var model = await _service.UpdateAddressAsync(item);
                _logger.LogInformation($"item {idArg} updated");
               
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(UpdateItem)}: {ex.Message}");
                return BadRequest($"Could not update. Error {ex.Message}");
            }
        }

        /// <summary>
        /// CREATE new item.
        /// Validates DTO, delegates to service, returns created model.
        /// </summary>
        [HttpPost()]
        [ActionName("CreateItem")]
        [ProducesResponseType(200, Type = typeof(IAddress))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> CreateItem([FromBody] AddressCuDto item)
        {
            try
            {
                // Validate required fields/rules before calling service
                item.EnsureValidity();

                _logger.LogInformation($"{nameof(CreateItem)}:");
               
                var model = await _service.CreateAddressAsync(item);
                _logger.LogInformation($"item {model.Item.AddressId} created");

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CreateItem)}: {ex.Message}");
                return BadRequest($"Could not create. Error {ex.Message}");
            }
        }

        /// <summary>
        /// Constructor injection of service + logger.
        /// </summary>
        public AddressesController(IAddressesService service, ILogger<AddressesController> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}
