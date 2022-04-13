using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkyApm.Diagnostic.Logger;
using SkyApm.Diagnostic.Logger.Transport.Grpc;
using SkyApm.Sample.Backend.Models;

namespace SkyApm.Sample.Backend.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private ILoggerReporter _logReporter;
        private IConfiguration _config;

        private ILogger _logger1;
        private IMyLogger _logger;
        public ValuesController(IConfiguration config,
            //IZwLogger logger,
            ILoggerFactory loggerFactory,
            ILoggerReporter logReporter)
        {
            _config = config;

            _logger = new MyLogger(loggerFactory.CreateLogger<ValuesController>(), typeof(ValuesController).FullName); //logger;
            _logReporter = logReporter;
        }
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logger.LogInformation("process ValuesController.Get success!");
            _logger.LogDebug("debug : process ValuesController.Get debug!");
            _logger.LogWarning("warning : process ValuesController.Get warning!");
            _logger.LogTrace("Trace : process ValuesController.Get trace!");
            _logger.LogError("error process ValuesController.Get error!",null);
            return new List<string> { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            _logger.LogInformation($"process ValuesController.Get({id}) success!");

            return "value";
        }

        [HttpGet("ignore")]
        public async Task<string> Ignore()
        {
            _logger.LogInformation("process ValuesController.Ignore success!");

            //System.IO.File.Copy("", "", true);
            //new System.IO.FileInfo("").MoveTo("", true);
            return "ignore";
        }

        [HttpGet("StopPropagation")]
        public string StopPropagation()
        {
            return "stop propagation";
        }

        [HttpPost("postin")]
        public string PostIn([FromBody] PostModel model)
        {
            return model.Name;
        }

        [HttpGet("ig")]
        public async Task<IActionResult> CallIgnore()
        {
            var message = await new HttpClient().GetStreamAsync("http://localhost:5002/api/values/ignore");
            _logger.LogInformation($"call HttpClient().GetStreamAsync(\"http://localhost:5002/api/values/ignore\") ,return {message}");

            return Ok(message);
        }

        [HttpGet("sp")]
        public async Task<IActionResult> CallStopPropagation()
        {
            var message = await new HttpClient().GetStreamAsync("http://localhost:5002/api/values/stoppropagation");
            _logger.LogInformation($"call HttpClient().GetStreamAsync(\"http://localhost:5002/api/values/stoppropagation\") ,return {message}");

            return Ok(message);
        }

        [HttpGet("ex")]
        public async Task<IActionResult> Throw()
        {
            throw new NotImplementedException();
        }

        [HttpGet("log")]
        public async Task Log()
        {
            string type = "Info";
            string content = "this is first log from dotnet core app";
            string traceId = Guid.NewGuid().ToString("d");//HttpContext.TraceIdentifier;// "428c0fc2a5badba107cbb57d033bebc5.33.16493962082340001";
            string endpoint = HttpContext.Request.Path.ToUriComponent();
            var logReq = BuildLogRequest(type, content, traceId, endpoint);
            await _logReporter.ReportAsync(new List<LogReportRequest> { logReq });
        }

        private LogReportRequest BuildLogRequest(string type, string content, string traceId, string endpoint)
        {
            string serviceName = "asp-net-core-backend";//_config.GetValue<string>("SkyWalking:ServiceName");
            string instance = "192.168.61.216@8d2a6263b59148fbb744e2bf3c3b5a39";//_config.GetValue<string>("SkyWalking:ServiceInstanceName");
            var logReq = new LogReportRequest
            {
                Service = serviceName,//必填
                ServiceInstance = instance,//必填
                Endpoint = endpoint,//必填
                LogBody = new LogReportBody
                {
                    Content = content,//必填
                    Type = type,//必填
                    TypeEnum = LogTypeEnum.Text
                },
                TraceContext = new LogTraceContext
                {
                    TraceId = traceId,//必填， 在日志中可查，在request的右上角按钮可查
                    SpanId = 0, //不填为0，request的弹窗相关日志查询
                    TraceSegmentId = traceId, //必填，request的弹窗相关日志查询
                },
                Layer = "ID: 2, NAME: general"
            };
            /*
            level = INFO
            logger = com.zwcad.cad2d.dwg.service.impl.SysDocumentServiceImpl
            thread = http - nio - 9090 - exec - 7
            */
            logReq.LogTags.Add(new KeyValuePair<string, string>("level", "INFO"));
            logReq.LogTags.Add(new KeyValuePair<string, string>("logger", "test.logger"));
            return logReq;
        }
    }
}