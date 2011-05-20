using System.Diagnostics;
using Bottles.Deployment.Directives;

namespace Bottles.Deployers.CommandLine
{    
    public class ExecuteCommandLine 
    {
        public void Execute(CommandLineExecution input)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = input.FileName,
                Arguments = input.Arguments,
                WorkingDirectory = input.WorkingDirectory,
                ErrorDialog = false                
            };
            Process.Start(processStartInfo);
        }
    }
}