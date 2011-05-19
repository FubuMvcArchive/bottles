using System.ComponentModel;
using Bottles.Deployment.Bootstrapping;
using Bottles.Deployment.Runtime;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class DeployInput
    {
        public DeployInput()
        {
            ProfileFlag = "default";
            ReportFlag = "installation_report.htm";
        }

        [Description("The profile to execute.  'default' is the default.")]
        public string ProfileFlag { get; set; }

        [Description("Path to where the deployment folder is ~/deployment")]
        public string DeploymentFlag { get; set; }

        [Description("File where the installation report should be written.  Default is installation_report.htm")]
        public string ReportFlag { get; set; }
    }

    [CommandDescription("Deploys the given profile")]
    public class DeployCommand : FubuCommand<DeployInput>
    {
        public override bool Execute(DeployInput input)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);

            var options = new DeploymentOptions(input.DeploymentFlag)
            {
                ReportName = input.ReportFlag,
                ProfileName = input.ProfileFlag
            };

            DeploymentBootstrapper.UsingService<IDeploymentController>(settings, x => x.Deploy(options));
             
            // TODO -- need to blow up / fail if there were any errors detected

            return true;
        }
    }
}