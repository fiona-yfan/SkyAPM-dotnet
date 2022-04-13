using System;
using System.Collections.Generic;
using System.Text;
using SkyApm.Diagnostics;
using SkyApm.Tracing;

namespace SkyApm.Diagnostic.Logger
{
    public class MyLogDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = LoggerDiagnosticConstants.ListenerName;


        // private readonly ITracingContext _tracingContext;
        private readonly IEntrySegmentContextAccessor _contextAccessor;
        //private readonly ILogDiagnosticHandler _diagnosticHandler;
        private readonly ILoggerDispatcher _dispatcher;
        public MyLogDiagnosticProcessor(
         //ITracingContext tracingContext
         IEntrySegmentContextAccessor contextAccessor
         , ILoggerDispatcher loggerDispatcher
        // , ILogDiagnosticHandler diagnosticHandler
        )
        {
            // _tracingContext = tracingContext;
            _dispatcher = loggerDispatcher;
            _contextAccessor = contextAccessor;
            // _diagnosticHandler = diagnosticHandler;
        }


        [DiagnosticName(LoggerDiagnosticConstants.DiagnosticName)]
        public void Process([Object] MyLogDetail detail)
        {
            if (detail == null || string.IsNullOrWhiteSpace(detail.OperationName))
                return;
            //var segment = _tracingContext.CreateLocalSegmentContext(detail.OperationName);
            // read EntrySegmentContext to get original TraceId
            var segment = _contextAccessor.Context;

            LoggerContext context = new LoggerContext(
                segment.TraceId,
                segment.SegmentId,
                true,
                segment.ServiceId,
                segment.ServiceInstanceId,
                detail.OperationName,
                detail.Content,
                detail.LogLevel
            );
            context.Tags.Add(new KeyValuePair<string, string>("Level", detail.LogLevel));
            context.Tags.Add(new KeyValuePair<string, string>("Logger", detail.Logger));
            _dispatcher.Dispatch(context);

        }
    }
}
