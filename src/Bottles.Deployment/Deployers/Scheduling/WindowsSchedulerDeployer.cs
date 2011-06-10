using System;
using System.Diagnostics;
using Bottles.Deployment.Runtime;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployment.Deployers.Scheduling
{
    public class WindowsSchedulerDeployer : IDeployer<ScheduledTask>
    {
        private readonly IProcessRunner _runner;
        private readonly IBottleMover _mover;

        public WindowsSchedulerDeployer(IProcessRunner runner, IBottleMover mover)
        {
            _runner = runner;
            _mover = mover;
        }

        public void Execute(ScheduledTask directive, HostManifest host, IPackageLog log)
        {
            var dest = new WinSchedBottleDestination(directive.InstallLocation);
            _mover.Move(log, dest, host.BottleReferences);

            var psi = new ProcessStartInfo("schtasks");

            var args = @"/create /tn {0} /sc {1} /mo {2} /ru {3} /tr {4} /F".ToFormat(directive.Name, directive.ScheduleType, directive.Modifier, directive.UserAccount, directive.TaskToRun);
            psi.Arguments = args;

            log.Trace(args);

            _runner.Run(psi, new TimeSpan(0, 0, 1, 0));
        }

    }
}