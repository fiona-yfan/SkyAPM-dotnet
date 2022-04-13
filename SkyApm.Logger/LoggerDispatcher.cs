using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SkyApm.Config;
using SkyApm.Diagnostic.Logger.Transport.Grpc;
using SkyApm.Logging;
using SkyApm.Transport;

namespace SkyApm.Diagnostic.Logger
{
    public class AsyncQueueLoggerDispatcher : ILoggerDispatcher
    {

        private readonly ILogger _logger;
        private readonly TransportConfig _config;
        private readonly ILoggerReporter _loggerReporter;
        private readonly ConcurrentQueue<LogReportRequest> _logReportQueue;
        private readonly IRuntimeEnvironment _runtimeEnvironment;
        private readonly CancellationTokenSource _cancellation;
        private int _offset;

        public AsyncQueueLoggerDispatcher(IConfigAccessor configAccessor,
            ILoggerReporter loggerReporter, IRuntimeEnvironment runtimeEnvironment,
            ISegmentContextMapper segmentContextMapper, ILoggerFactory loggerFactory)
        {
            _loggerReporter = loggerReporter;
            _runtimeEnvironment = runtimeEnvironment;
            _logger = loggerFactory.CreateLogger(typeof(AsyncQueueLoggerDispatcher));
            _config = configAccessor.Get<TransportConfig>();
            _logReportQueue = new ConcurrentQueue<LogReportRequest>();
            _cancellation = new CancellationTokenSource();
        }

        public void Close()
        {
            _cancellation.Cancel();
        }

        public bool Dispatch(LoggerContext loggerContext)
        {
            if (!_runtimeEnvironment.Initialized || loggerContext == null || !loggerContext.Sampled)
            {
                return false;
            }

            if (_config.QueueSize < _offset || _cancellation.IsCancellationRequested)
            {
                return false;
            }

            var logRequest = Map(loggerContext);
            if (logRequest == null) return false;

            _logReportQueue.Enqueue(logRequest);

            Interlocked.Increment(ref _offset);

            _logger.Debug($"Dispatch logger context. [SpanId]={loggerContext.SpanId}.");
            return true;
        }


        public Task Flush(CancellationToken token = default)
        {
            var limit = _config.BatchSize;
            var index = 0;
            var logRequest = new List<LogReportRequest>(limit);
            while (index++ < limit && _logReportQueue.TryDequeue(out var request))
            {
                logRequest.Add(request);
                Interlocked.Decrement(ref _offset);
            }

            if (logRequest.Count > 0)
            {
                _loggerReporter.ReportAsync(logRequest, token);
            }

            Interlocked.Exchange(ref _offset, _logReportQueue.Count);
            return Task.CompletedTask;

        }

        private LogReportRequest Map(LoggerContext context)
        {
            if (context == null)
                return null;

            var request = new LogReportRequest
            {
                TraceContext = new LogTraceContext
                {
                    TraceId = context.TraceId,
                    SpanId = context.SpanId,
                    TraceSegmentId = context.SegmentId
                },
                Service = context.ServiceId,
                ServiceInstance = context.ServiceInstanceId,
                Endpoint = context.OperationName,
                Layer = "ID: 2, NAME: general",
                Timestamp = context.Timestamp,
                LogBody = new LogReportBody
                {
                    Content = context.Content,
                    Type = context.LogType,
                    TypeEnum = LogTypeEnum.Text
                },

            };
            if (context.Tags != null && context.Tags.Any()) {
                foreach (var kv in context.Tags)
                {
                    request.LogTags.Add(kv);
                }
            }

            return request;
        }

    }
}
