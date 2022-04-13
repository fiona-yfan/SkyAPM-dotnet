using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Diagnostic.Logger.Transport.Grpc
{
   public interface ILoggerReporter
    {
        Task ReportAsync(IReadOnlyCollection<LogReportRequest> logRequests,
             CancellationToken cancellationToken = default(CancellationToken));

    }

    
}
