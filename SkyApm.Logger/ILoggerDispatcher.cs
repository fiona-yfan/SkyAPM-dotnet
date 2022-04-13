using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkyApm.Diagnostic.Logger
{
    public interface ILoggerDispatcher
    {
        bool Dispatch(LoggerContext loggerContext);

        Task Flush(CancellationToken token = default(CancellationToken));

        void Close();
    }
}
