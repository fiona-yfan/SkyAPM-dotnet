using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Diagnostic.Logger.Transport.Grpc
{
    public class LogReportRequest
    {
        public long Timestamp { get; set; }
        public string Service { get; set; }
        public string ServiceInstance { get; set; }
        public string Endpoint { get; set; }
        public LogReportBody LogBody { get; set; }
        public LogTraceContext TraceContext { get; set; }
        public IList<KeyValuePair<string, string>> LogTags { get; } = new List<KeyValuePair<string, string>>();
        public string Layer { get; set; }

    }

    public class LogReportBody
    {
        public string Type { get; set; }

        public string Content { get; set; }
        public LogTypeEnum TypeEnum { get; set; }

    }

    public class LogTraceContext
    {
        public string TraceId { get; set; }
        public string TraceSegmentId { get; set; }
        public int SpanId { get; set; }
    }
    
    public enum LogTypeEnum
    {
        None = 0,
        Text = 2,
        Json = 3,
        Yaml = 4,
    }
}