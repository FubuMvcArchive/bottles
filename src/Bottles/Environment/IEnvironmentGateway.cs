using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles.Environment
{
    public interface IEnvironmentGateway
    {
        IEnumerable<EnvironmentLogEntry> Install();
        IEnumerable<EnvironmentLogEntry> CheckEnvironment();
        IEnumerable<EnvironmentLogEntry> InstallAndCheckEnvironment();
    }
}