using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles.Deployment.Commands
{
    public interface IInstallationLogger
    {
        void WriteLogsToConsole(IEnumerable<LogEntry> entries);
        void WriteLogsToFile(InstallInput input, IEnumerable<LogEntry> entries);
        void WriteSuccessToConsole();
        void WriteFailureToConsole();
    }
}