using FubuCore;

namespace Bottles.Deployment.Directives
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