using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using Models.DTO;
using Services;
using Configuration;
using Configuration.Options;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AdminController : Controller
    {
        readonly DatabaseConnections _dbConnections;
        readonly IAdminService _service;
        readonly ILogger<AdminController> _logger;
        readonly VersionOptions _versionOptions;
        readonly EnvironmentOptions _environmentOptions;

#if DEBUG
        //GET: api/admin/environment
        [HttpGet()]
        [ActionName("Environment")]
        [ProducesResponseType(200, Type = typeof(EnvironmentOptions))]
        public IActionResult Environment()
        {
            try
            {
                var info = _environmentOptions;

                _logger.LogInformation($"{nameof(Environment)}:\n{JsonConvert.SerializeObject(info)}");
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(Environment)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
         }

        //GET: api/admin/log
        [HttpGet()]
        [ActionName("Log")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<LogMessage>))]
        public async Task<IActionResult> Log([FromServices] ILoggerProvider _loggerProvider)
        {
            //Note the way to get the LoggerProvider, not the logger from Services via DI
            if (_loggerProvider is InMemoryLoggerProvider cl)
            {
                return Ok(await cl.MessagesAsync);
            }
            return Ok("No messages in log");
        }
#endif

        [HttpGet()]
        [ActionName("Seed")]
        [ProducesResponseType(200, Type = typeof(GstUsrInfoAllDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> Seed(string count = "100")
        {
            try
            {
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
                _logger.LogError(ex, "Error retrieving version information");
                return BadRequest(ex.Message);
            }
        }

        public AdminController(IAdminService service, ILogger<AdminController> logger,
                DatabaseConnections dbConnections, IOptions<VersionOptions> versionOptions, IOptions<EnvironmentOptions> environmentOptions)
        {
            _service = service;
            _logger = logger;
            _dbConnections = dbConnections;
            _versionOptions = versionOptions.Value;
            _environmentOptions = environmentOptions.Value;
        }
    }
}

