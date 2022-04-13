using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SkyApm.Logging;

namespace SkyApm.Diagnostic.Logger
{
    public class LoggerService : ExecutionService
    {

        private readonly ILoggerDispatcher _dispatcher;
        public LoggerService(IRuntimeEnvironment runtimeEnvironment, ILoggerFactory loggerFactory
        , ILoggerDispatcher dispatcher) : base(runtimeEnvironment, loggerFactory)
        {
            _dispatcher = dispatcher;
        }

        protected override TimeSpan DueTime { get; } = TimeSpan.FromSeconds(3);
        protected override TimeSpan Period { get; } = TimeSpan.FromSeconds(3);
        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _dispatcher.Flush(cancellationToken);
        }
        protected override Task Stopping(CancellationToken cancellationToke)
        {
            _dispatcher.Close();
            return Task.CompletedTask;
        }
    }
}
