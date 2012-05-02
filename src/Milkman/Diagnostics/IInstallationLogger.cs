using System.Collections.Generic;
using Bottles.Deployment.Commands;
using Bottles.Diagnostics;
using Bottles.Environment;

namespace Bottles.Deployment.Diagnostics
{
    public interface IInstallationLogger
    {
        void WriteLogsToConsole(IEnumerable<EnvironmentLogEntry> entries);
        void WriteLogsToFile(InstallInput input, IEnumerable<EnvironmentLogEntry> entries);
        void WriteSuccessToConsole();
        void WriteFailureToConsole();
    }
}