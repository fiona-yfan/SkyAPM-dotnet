using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkyApm.Diagnostic.Logger

{

    public interface IMyLogger
    {
        void LogDebug(string message);

        void LogInformation(string message);

        void LogWarning(string message);

        void LogError(string message, Exception exception);

        void LogTrace(string message);
    }
    public class MyLogger : IMyLogger
    {

        private readonly DiagnosticListener _loggerDiagnosticListener = new DiagnosticListener(LoggerDiagnosticConstants.ListenerName);
        private readonly ILogger _logger;

        private readonly string _category;
        //public ZwLogger(ILogger<ZwLogger> logger)
        //{
        //    _logger = logger;
        //}

        public MyLogger(ILogger logger, string typeName = default)
        {
            _logger = logger;
            _category = typeName;
        }
        public void LogDebug(string message)
        {
            _logger.LogDebug(message);

            Writer(new MyLogDetail
            {
                Content = message,
                LogLevel = "Debug",
                OperationName = "LogDebug",
                Logger = _category
            });
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
            Writer(new MyLogDetail
            {
                Content = message,
                LogLevel = "Info",
                OperationName = "LogInformation",
                Logger = _category
            });


        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
            Writer(new MyLogDetail
            {
                Content = message,
                LogLevel = "Warning",
                OperationName = "LogWarning",
                Logger = _category
            });

        }

        public void LogError(string message, Exception exception = null)
        {
            _logger.LogError(exception, message);
            Writer(new MyLogDetail
            {
                Content = $"{message}{Environment.NewLine} StackTrace:{exception?.ToString()}",
                LogLevel = "Error",
                OperationName = "LogError",
                Logger = _category
            });

        }

        public void LogTrace(string message)
        {
            _logger.LogTrace(message);
            Writer(new MyLogDetail
            {
                Content = message,
                LogLevel = "Trace",
                OperationName = "LogTrace",
                Logger = _category
            });


        }

        private void Writer(MyLogDetail detail)
        {
            if (_loggerDiagnosticListener.IsEnabled(LoggerDiagnosticConstants.DiagnosticName))
                _loggerDiagnosticListener.Write(LoggerDiagnosticConstants.DiagnosticName, detail);
        }

    }

    public class MyLogDetail
    {
        /// <summary>
        /// for type
        /// </summary>
        public string LogLevel { get; set; }
        /// <summary>
        /// for EndPiont
        /// </summary>
        public string OperationName { get; set; }
        public string Content { get; set; }
        public long Timestamp { get; set; } = DateTime.Now.Ticks;
        // public long ThreadId { get; set; }

        // public string MethodName { get; set; }
        //public string StackTrace { get; set; }
        public string Logger { get; set; }


    }
    public class LoggerDiagnosticConstants
    {
        public const string DiagnosticName = "My.Logger.Log";
        public const string ListenerName = "My.Logger.Listener";
    }
}
