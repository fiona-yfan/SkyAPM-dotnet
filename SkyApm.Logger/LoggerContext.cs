using System.Collections.Generic;

namespace SkyApm.Diagnostic.Logger
{
    public class LoggerContext
    {
        public string SegmentId { get; }
        public int SpanId { get; }

        public string TraceId { get; }

        //public SegmentSpan Span { get; }

        public string ServiceId { get; }

        public string ServiceInstanceId { get; }

        public bool Sampled { get; }

        public bool IsSizeLimited { get; } = false;

        public string OperationName { get; }
        public string LogType { get; }
        public string Content { get; }
        public IList<KeyValuePair<string, string>> Tags { get; }

        public long Timestamp { get; set; }

        //public SegmentReferenceCollection References { get; } = new SegmentReferenceCollection();

        public LoggerContext(string traceId, string segmentId, bool sampled, string serviceId, string serviceInstanceId,
            string operationName, string content, string logType
            , IList<KeyValuePair<string, string>> tags = default
            , long startTimeMilliseconds = default)
        {
            TraceId = traceId;
            Sampled = sampled;
            SegmentId = segmentId;
            ServiceId = serviceId;
            ServiceInstanceId = serviceInstanceId;
            LogType = logType;
            Content = content;
            OperationName = operationName;
            Tags = tags ?? new List<KeyValuePair<string, string>>();
            Timestamp = startTimeMilliseconds;
            //Span = new SegmentSpan(operationName, spanType, startTimeMilliseconds);
        }
    }


}