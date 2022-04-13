using Serilog.Core;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.IO;
using Serilog.Formatting;
using System.Collections.Generic;
using System.Linq;

namespace Serilog.Sinks.DiagnosticSource
{
    class DiagnosticSink : ILogEventSink
    {
        readonly ITextFormatter _textFormatter;

        private const string DiagnosticListenerName = "Serilog.Logger.DiagnosticListener";
        private const string DiagnosticName = "Serilog.Logger.Log";
        private string[] IgnoreSourceContexts = { "Microsoft.AspNetCore.Server.Kestrel", "Microsoft.Hosting.Lifetime", "Microsoft.EntityFrameworkCore.Infrastructure" };
        private string[] AllowTags = { "ActionName", "RequestPath", "EventId", "SpanId" };

        static readonly DiagnosticListener LoggerDiagnosticListener = new DiagnosticListener(DiagnosticListenerName);

        //private readonly IHttpContextAccessor _httpContextAccessor;
        public DiagnosticSink(ITextFormatter textFormatter)
        //, IHttpContextAccessor httpContextAccessor)
        {
            if (textFormatter == null) throw new ArgumentNullException(nameof(textFormatter));
            _textFormatter = textFormatter;
            //_httpContextAccessor = httpContextAccessor;
        }
        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            var sr = new StringWriter();
            _textFormatter.Format(logEvent, sr);

            var text = sr.ToString().Trim();
            sr.Dispose();

            if (CanWrite(logEvent.Properties) && LoggerDiagnosticListener.IsEnabled(DiagnosticName))
            {
                // use anonymous object
                var detail = new
                {
                    Content = text,
                    Timestamp = logEvent.Timestamp.ToUnixTimeMilliseconds(),
                    Level = logEvent.Level.ToString(),
                    Properties = GetAllProperties(logEvent),
                    OperationName = $"Log{logEvent.Level}"
                };
                LoggerDiagnosticListener.Write(DiagnosticName, detail);
            }
        }

        private List<KeyValuePair<string, string>> GetAllProperties(LogEvent logEvent)
        {
            if (logEvent == null || logEvent.Properties == null || logEvent.Properties.Count == 0)
                return null;

            var props = new Dictionary<string, string>();
            foreach (var prop in logEvent.Properties)
            {

                if (AllowTags.Contains(prop.Key))
                    props.Add(prop.Key, prop.Value.ToString().Trim('"'));
            }
            return props.ToList();
        }


        private bool CanWrite(IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {

            bool canWrite = true;

            if (properties.TryGetValue("SourceContext", out var propertyVal) && propertyVal is ScalarValue pVal)
            {
                canWrite = !IgnoreSourceContexts.Contains(GetScalarValue(pVal));
            }

            return canWrite;
        }

        private string GetScalarValue(ScalarValue value)
        {
            return value?.Value?.ToString();
        }
    }

}
