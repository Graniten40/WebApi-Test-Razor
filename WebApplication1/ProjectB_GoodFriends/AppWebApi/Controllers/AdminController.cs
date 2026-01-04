using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

using Models.DTO;
using Services.Interfaces;
using Configuration;
using Configuration.Options;
using Microsoft.Extensions.Options;


namespace AppWebApi.Controllers
{
    /// <summary>
    /// Admin controller for operational endpoints (seed, version, environment, logs).
    /// Routing uses "api/[controller]/[action]" so endpoints become:
    /// - api/Admin/Version
    /// - (DEBUG only) api/Admin/Environment, api/Admin/Seed, api/Admin/RemoveSeed, api/Admin/Log
    /// 
    /// These endpoints are typically used during development/testing to:
    /// - seed or clear test data
    /// - verify configuration/environment
    /// - inspect in-memory logs
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AdminController : Controller
    {
        // Holds configured database connection info (useful for diagnostics / environment understanding)
        readonly DatabaseConnections _dbConnections;

        // Service layer encapsulates admin operations (seed/remove seed, etc.)
        readonly IAdminService _service;

        // Logger for traceability and debugging
        readonly ILogger<AdminController> _logger;

        // Strongly typed options bound from configuration (appsettings, environment vars, etc.)
        readonly VersionOptions _versionOptions;
        readonly EnvironmentOptions _environmentOptions;

#if DEBUG
        /// <summary>
        /// DEBUG-only endpoint: returns EnvironmentOptions for inspection during development.
        /// Wrapped in #if DEBUG so it cannot be used in Release deployments.
        /// </summary>
        [HttpGet()]
        [ActionName("Environment")]
        [ProducesResponseType(200, Type = typeof(EnvironmentOptions))]
        public IActionResult Environment()
        {
            try
            {
                var info = _environmentOptions;

                // Serialize options to log for easy copy/paste debugging
                _logger.LogInformation($"{nameof(Environment)}:\n{JsonConvert.SerializeObject(info)}");
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(Environment)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
         }

        /// <summary>
        /// DEBUG-only endpoint: seeds the database with test data.
        /// "count" controls how many items to seed (defaults to 100).
        /// </summary>
        [HttpGet()]
        [ActionName("Seed")]
        [ProducesResponseType(200, Type = typeof(GstUsrInfoAllDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> Seed(string count = "100")
        {
            try
            {
                // Parse querystring count to int
                int countArg = int.Parse(count);

                _logger.LogInformation($"{nameof(Seed)}: {nameof(countArg)}: {countArg}");
                var info = await _service.SeedAsync(countArg);
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(Seed)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// DEBUG-only endpoint: removes seed/test data.
        /// "seeded" indicates which dataset to remove (defaults to true).
        /// </summary>
        [HttpGet()]
        [ActionName("RemoveSeed")]
        [ProducesResponseType(200, Type = typeof(GstUsrInfoAllDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> RemoveSeed(string seeded = "true")
        {
            try
            {
                bool seededArg = bool.Parse(seeded);

                _logger.LogInformation($"{nameof(RemoveSeed)}: {nameof(seededArg)}: {seededArg}");
                var info = await _service.RemoveSeedAsync(seededArg);
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(RemoveSeed)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// DEBUG-only endpoint: returns messages from an in-memory logger provider (if enabled).
        /// Uses [FromServices] to request ILoggerProvider directly for this action.
        /// </summary>
        [HttpGet()]
        [ActionName("Log")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<LogMessage>))]
        public async Task<IActionResult> Log([FromServices] ILoggerProvider _loggerProvider)
        {
            // Note: this retrieves the provider itself (not ILogger<T>) so we can access stored messages.
            if (_loggerProvider is InMemoryLoggerProvider cl)
            {
                return Ok(await cl.MessagesAsync);
            }
            return Ok("No messages in log");
        }

#endif

        /// <summary>
        /// Public endpoint: returns version/build information via VersionOptions.
        /// Useful for verifying deployed version and configuration.
        /// </summary>
        [HttpGet()]
        [ActionName("Version")]
        [ProducesResponseType(typeof(VersionOptions), 200)]
        public IActionResult Version()
        {
            try
            {
                _logger.LogInformation($"{nameof(Version)}:\n{JsonConvert.SerializeObject(_versionOptions)}");
                return Ok(_versionOptions);
            }
            catch (Exception ex)
            {
                // Logs exception object for stacktrace/details
                _logger.LogError(ex, "Error retrieving version information");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Constructor injection of:
        /// - IAdminService: admin operations
        /// - ILogger: logging
        /// - DatabaseConnections + Options: strongly typed config
        /// </summary>
        public AdminController(IAdminService service, ILogger<AdminController> logger,
                DatabaseConnections dbConnections, IOptions<VersionOptions> versionOptions,
                IOptions<EnvironmentOptions> environmentOptions)
        {
            _service = service;
            _logger = logger;
            _dbConnections = dbConnections;

            // IOptions<T>.Value contains the final bound configuration object
            _versionOptions = versionOptions.Value;
            _environmentOptions = environmentOptions.Value;
        }
    }
}
