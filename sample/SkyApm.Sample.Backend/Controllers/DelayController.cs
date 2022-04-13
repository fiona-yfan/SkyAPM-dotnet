using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SkyApm.Sample.Backend.Controllers
{
    //[Route("api/[controller]")]
    public class DelayController : Controller
    {
        private readonly ILogger<DelayController> _logger;
        public DelayController(ILogger<DelayController> logger)
        {
            _logger = logger;
        }
        // GET
        [HttpGet("api/[controller]/{delay}")]
        public async Task<string> Get(int delay)
        {
            await Task.Delay(delay);
            return $"delay {delay}ms";
        }


        [HttpGet("api/log")]
        public IActionResult Log()
        {
            _logger.LogInformation("this is test serilog diagnostic LogInformation");
            _logger.LogWarning("this is test serilog diagnostic LogWarning");
            _logger.LogDebug("this is test serilog diagnostic LogDebug");
            _logger.LogTrace("this is test serilog diagnostic LogTrace");
            _logger.LogError("this is test serilog diagnostic LogError");
            _logger.LogCritical("this is test serilog diagnostic LogCritical");

            return Ok("log");
        }
    }
}