using FubuCore;

namespace Bottles.Deployment.Deployers.CommandLine
{
    public class CommandLineExecution : IDirective
    {
        public CommandLineExecution()
        {
            WorkingDirectory = ".".ToFullPath();
        }

        public string FileName { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
    }
}