using Grpc.Core;
using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Transport.Grpc;
using SkyWalking.NetworkProtocol.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Diagnostic.Logger.Transport.Grpc
{
    public class LoggerReporter : ILoggerReporter
    {
        private readonly ConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly GrpcConfig _config;
        //private readonly IRuntimeEnvironment _runtimeEnvironment;
        //private readonly InstrumentConfig _instrumentConfig;


        public LoggerReporter(ConnectionManager connectionManager,
          IConfigAccessor configAccessor, ILoggerFactory loggerFactory)
        {
            _connectionManager = connectionManager;
            _logger = loggerFactory.CreateLogger(typeof(LoggerReporter));
            _config = configAccessor.Get<GrpcConfig>();
            //_runtimeEnvironment = runtimeEnvironment;
            //_instrumentConfig = configAccessor.Get<InstrumentConfig>();
        }

        public async Task ReportAsync(IReadOnlyCollection<LogReportRequest> logRequests, CancellationToken cancellationToken = default)
        {
            Channel connection = null;
            if (!_connectionManager.Ready)
            {
                return;
            }

            connection = _connectionManager.GetConnection();

            var client = new LogReportService.LogReportServiceClient(connection);
            var metadata = _config.GetMeta();

            using (var asyncClientStreamingCall =
                   client.collect(_config.GetMeta(), _config.GetReportTimeout(), cancellationToken))
            {
                foreach (var logData in logRequests)
                    await asyncClientStreamingCall.RequestStream.WriteAsync(Map(logData));
                await asyncClientStreamingCall.RequestStream.CompleteAsync();
                await asyncClientStreamingCall.ResponseAsync;
            }
        }

        private LogData Map(LogReportRequest logRequest)
        {
            var log = new LogData
            {
                Timestamp = logRequest.Timestamp,
                Service = logRequest.Service,
                ServiceInstance = logRequest.ServiceInstance,
                Layer = logRequest.Layer,
                Endpoint = logRequest.Endpoint,

            };
            log.Body = MapLogBody(logRequest.LogBody);
            log.TraceContext = MapLogTraceContext(logRequest.TraceContext);
            log.Tags = MapLogTags(logRequest.LogTags);

            return log;
        }

        private LogTags MapLogTags(IList<KeyValuePair<string, string>> tags)
        {
            if (tags != null && tags.Count > 0)
            {
                var logTags = new LogTags();
                logTags.Data.Add(tags.Select(t => new KeyStringValuePair { Key = t.Key, Value = t.Value ?? string.Empty }));
                return logTags;
            }
            return null;
        }

        private TraceContext MapLogTraceContext(LogTraceContext traceContext)
        {
            if (traceContext == null) return null;

            return new TraceContext
            {
                SpanId = traceContext.SpanId,
                TraceSegmentId = traceContext.TraceSegmentId,
                TraceId = traceContext.TraceId,
            };
        }

        private LogDataBody MapLogBody(LogReportBody logBody)
        {
            if (logBody == null) return null;

            var body = new LogDataBody();
            body.Type = logBody.Type;
            switch (logBody.TypeEnum)
            {
                case LogTypeEnum.Text:
                    body.Text = new TextLog { Text = logBody.Content };
                    break;
                case LogTypeEnum.Json:
                    body.Json = new JSONLog { Json = logBody.Content };
                    break;
                case LogTypeEnum.Yaml:
                    body.Yaml = new YAMLLog { Yaml = logBody.Content };
                    break;
                default:
                    break;
            }
            return body;   
        }
    }
}
