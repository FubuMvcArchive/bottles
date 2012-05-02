using System;
using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles.Environment
{
    public class EnvironmentProxy : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public IEnumerable<EnvironmentLogEntry> Install(EnvironmentRun run)
        {
            return execute(run, (installer, log) => installer.Install(log));
        }

        public IEnumerable<EnvironmentLogEntry> CheckEnvironment(EnvironmentRun run)
        {
            return execute(run, (installer, log) => installer.CheckEnvironment(log));
        }

        public IEnumerable<EnvironmentLogEntry> InstallAndCheckEnvironment(EnvironmentRun run)
        {
            return execute(run, (installer, log) => installer.Install(log),
                                (installer, log) => installer.CheckEnvironment(log));
        }

        private IEnumerable<EnvironmentLogEntry> execute(EnvironmentRun run, params Action<IInstaller, IPackageLog>[] actions)
        {
            var runner = new EnvironmentRunner(run);
            return runner.ExecuteEnvironment(actions);
        }
    }
}