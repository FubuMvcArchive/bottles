using System.Diagnostics;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;

namespace Bottles.Deployment.Deployers.CommandLine
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
            var processStartInfo = new ProcessStartInfo
            {
                FileName = directive.FileName,
                Arguments = directive.Arguments,
                WorkingDirectory = directive.WorkingDirectory,
                ErrorDialog = false
            };

            log.Trace("Executing the command '{0}' with args '{1}'", directive.FileName, directive.Arguments);
            var exitCode = _processRunner.Run(processStartInfo); 
            log.Trace("Command completed with exit code '{0}'", exitCode);
        }
    }
}
