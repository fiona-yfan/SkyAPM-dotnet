using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using SkyApm.Diagnostic.Logger.Transport.Grpc;

namespace SkyApm.Diagnostic.Logger
{
   public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDiagnosticLogger(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerReporter, LoggerReporter>();
            services.AddSingleton<IExecutionService, LoggerService>();
            services.AddSingleton<ILoggerDispatcher, AsyncQueueLoggerDispatcher>();
            services.AddSingleton<ITracingDiagnosticProcessor, MyLogDiagnosticProcessor>();
            services.AddSingleton<ITracingDiagnosticProcessor, SerilogDiagnosticProcessor>();

            // todo rework ILogger diagnostic
            //services.AddTransient<IZwLogger, ZwLogger>();
            return services;
        }
    }
}
