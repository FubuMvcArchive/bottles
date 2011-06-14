using System.Diagnostics;
using System.IO;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;

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
            ProcessStartInfo processStartInfo = GetProcessStartInfo(directive);

            log.Trace("Executing the command '{0}' with args '{1}'", directive.FileName, directive.Arguments);
            var exitCode = _processRunner.Run(processStartInfo); 
            log.Trace("Command completed with exit code '{0}'", exitCode);
        }

        public ProcessStartInfo GetProcessStartInfo(CommandLineExecution directive)
        {
            var fileName = directive.FileName;
            if (!Path.IsPathRooted(fileName))
            {
                fileName = directive.WorkingDirectory.AppendPath(directive.FileName);
            }

            return new ProcessStartInfo
                   {
                       FileName = fileName,
                       Arguments = directive.Arguments,
                       WorkingDirectory = directive.WorkingDirectory,
                       ErrorDialog = false
                   };
        }
    }
}
