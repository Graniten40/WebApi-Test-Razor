using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using Models.DTO;
using Services;
using System.Text.RegularExpressions;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class GuestController : Controller
    {
        readonly IAdminService _service;
        readonly ILoginService _loginService;
        readonly ILogger<GuestController> _logger = null;

        //GET: api/guest/info
        [HttpGet()]
        [ActionName("Info")]
        [ProducesResponseType(200, Type = typeof(GstUsrInfoAllDto))]
        public async Task<IActionResult> Info()
        {
            try
            {
                var info = await _service.GuestInfoAsync();

                _logger.LogInformation($"{nameof(Info)}:\n{JsonConvert.SerializeObject(info)}");
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(Info)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        public GuestController(IAdminService service, ILoginService loginService,
                ILogger<GuestController> logger)
        {
            _service = service;
            _loginService = loginService;
            _logger = logger;
        }
    }
}

