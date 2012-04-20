using System.ComponentModel;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class CopyInput
    {
        [Description("The directory name where the deployment artifacts are going to be written")]
        [FlagAlias("destination", 'o')]
        public string Destination { get; set; }

        [FlagAlias("create-bottles", 'c')]
        public bool CreateBottlesFlag { get; set; }

        [Description("Path to where the deployment folder is ~/deployment")]
        [FlagAlias("deployment", 'd')]
        public string DeploymentFlag { get; set; }
    }
}