using Bottles.Deployment;
using Bottles.Deployment.Directives;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;

namespace Bottles.Deployers.CommandLine
{
    public class CommandLineDeployer : IDeployer<CommandLineExecution>
    {
        private readonly IProcessRunner _processRunner;

        public CommandLineDeployer(IProcessRunner processRunner)
        {
            _processRunner = processRunner;
        }

        public void Execute(CommandLineExecution directive, HostManifest host, IPackageLog log)
        {
            new ExecuteCommandLine(_processRunner).Execute(directive);
        }
    }
}
