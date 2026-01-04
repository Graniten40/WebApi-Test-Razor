using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

using Models.DTO;
using Services.Interfaces;
using System.Text.RegularExpressions;

namespace AppWebApi.Controllers
{
    /// <summary>
    /// Guest controller for public/unauthenticated endpoints.
    /// Typically used by clients to fetch "guest info" / system status without requiring login.
    /// 
    /// Route pattern: "api/[controller]/[action]" => api/Guest/Info
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class GuestController : Controller
    {
        // AdminService is reused here to produce aggregated "guest info" response
        // (e.g. counts, environment/version-like data depending on implementation).
        readonly IAdminService _service;

        // Login service is injected (even if not used in this snippet),
        // typically for future/related guest operations (e.g., login endpoints).
        readonly ILoginService _loginService;

        // Logger for diagnostics and traceability.
        // Note: initialized to null here, but assigned via DI in constructor.
        readonly ILogger<GuestController> _logger = null;

        /// <summary>
        /// Returns guest information (public information for the frontend).
        /// Logs the payload in serialized form for debugging/visibility.
        /// </summary>
        [HttpGet()]
        [ActionName("Info")]
        [ProducesResponseType(200, Type = typeof(GstUsrInfoAllDto))]
        public async Task<IActionResult> Info()
        {
            try
            {
                // Delegate to service layer to build the guest response object
                var info = await _service.GuestInfoAsync();

                // Log serialized response for debugging (can be helpful during development)
                _logger.LogInformation($"{nameof(Info)}:\n{JsonConvert.SerializeObject(info)}");
                return Ok(info);
            }
            catch (Exception ex)
            {
                // Return 400 with error message on failures (simple error model for this project)
                _logger.LogError($"{nameof(Info)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Constructor injection of services + logger.
        /// DI provides the concrete implementations configured in the application.
        /// </summary>
        public GuestController(IAdminService service, ILoginService loginService,
                ILogger<GuestController> logger)
        {
            _service = service;
            _loginService = loginService;
            _logger = logger;
        }
    }
}
