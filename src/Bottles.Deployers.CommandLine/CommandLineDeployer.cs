using Bottles.Deployment;
using Bottles.Deployment.Directives;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;

namespace Bottles.Deployers.CommandLine
{
    public class CommandLineDeployer : IDeployer<CommandLineExecution>
    {
       
        public void Execute(CommandLineExecution directive, HostManifest host, IPackageLog log)
        {
            new ExecuteCommandLine().Execute(directive);
        }
    }
}
