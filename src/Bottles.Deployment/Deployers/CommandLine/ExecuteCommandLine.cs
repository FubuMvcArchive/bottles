using System.Diagnostics;

namespace Bottles.Deployment.Deployers.CommandLine
{    
    public class ExecuteCommandLine 
    {
        private readonly IProcessRunner _processRunner;

        public ExecuteCommandLine(IProcessRunner processRunner)
        {
            _processRunner = processRunner;
        }

        public void Execute(CommandLineExecution input)
        {
            
            var processStartInfo = new ProcessStartInfo
            {
                FileName = input.FileName,
                Arguments = input.Arguments,
                WorkingDirectory = input.WorkingDirectory,
                ErrorDialog = false                
            };
            _processRunner.Run(processStartInfo);            
        }
    }
}