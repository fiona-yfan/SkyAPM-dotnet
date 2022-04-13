using System;
using System.Collections.Generic;
using System.Text;
using SkyApm.Diagnostics;
using SkyApm.Tracing;

namespace SkyApm.Diagnostic.Logger
{
    public class SerilogDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = "Serilog.Logger.DiagnosticListener";

        private readonly IEntrySegmentContextAccessor _contextAccessor;
        private readonly ILoggerDispatcher _dispatcher;
        public SerilogDiagnosticProcessor(
         IEntrySegmentContextAccessor contextAccessor
         , ILoggerDispatcher loggerDispatcher
        )
        {
            _dispatcher = loggerDispatcher;
            _contextAccessor = contextAccessor;
        }


        [DiagnosticName("Serilog.Logger.Log")]
        public void Process([AnonymousObject] object detail)
        {
            // the entry segment content for per request
            var segment = _contextAccessor.Context;

            if (detail == null || segment == null)
                return;
            string name = string.Empty;
            string content = string.Empty;
            string level = string.Empty;
            long timestamp = default;
            IList<KeyValuePair<string, string>> properties = null;

            foreach (var prop in detail.GetType().GetProperties())
            {
                var val = prop.GetValue(detail);
                if (val != null)
                {
                    switch (prop.Name)
                    {
                        case "OperationName":
                        case "Name":
                        case "Endpoint":
                            name = val?.ToString();
                            break;
                        case "Content":
                        case "Message":
                            content = val?.ToString();
                            break;
                        case "Level":
                            level = val?.ToString();
                            break;
                        case "Timestamp":
                            timestamp = (long)val;
                            break;
                        case "Properties":
                            properties = val as IList<KeyValuePair<string, string>>;
                            break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(level) ||
                string.IsNullOrWhiteSpace(content)) return;
            LoggerContext context = new LoggerContext(
                segment.TraceId,
                segment.SegmentId,
                true,
                segment.ServiceId,
                segment.ServiceInstanceId,
                name,
                content,
                level,
                properties,
                timestamp
            );
            context.Tags.Add(new KeyValuePair<string, string>("Level", level));
            _dispatcher.Dispatch(context);

        }
    }
}
