using FubuCore;

namespace Bottles.Deployment.Deployers.CommandLine
{
    public class CommandLineExecution : IDirective
    {
        public CommandLineExecution()
        {
            WorkingDirectory = ".".ToFullPath();
            TimeoutInSeconds = 60;
        }

        public string FileName { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public int TimeoutInSeconds { get; set; }
    }
}