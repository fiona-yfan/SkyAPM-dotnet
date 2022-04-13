using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkyApm.Diagnostic.Logger;

namespace SkyApm.Sample.Backend.Controllers
{
    [Route("api/[controller]")]
    public class ErrorsController : Controller
    {
        private ILogger<ErrorsController> _logger;
        public ErrorsController(
            ILogger<ErrorsController> logger)
        //ILoggerFactory loggerFactory)
        {
            _logger = logger;
            //_logger = new MyLogger(loggerFactory.CreateLogger<ValuesController>(), typeof(ValuesController).FullName); //logger;
        }
        public string Get()
        {
            throw new InvalidOperationException("error sample.");
        }

        [Route("error")]
        public IActionResult error()
        {
            var exHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;
            _logger.LogError("system error", exHandlerFeature.Error);
            return Problem(title: "error handle", detail: exHandlerFeature.Error.ToString(), statusCode: StatusCodes.Status500InternalServerError);
            //return StatusCode(StatusCodes.Status500InternalServerError, new { exHandlerFeature.Error.Message, StackTrace = exHandlerFeature.Error.ToString() });
        }
    }
}