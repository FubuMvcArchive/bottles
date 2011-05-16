using System.ComponentModel;
using Bottles.Deployment.Bootstrapping;
using Bottles.Deployment.Runtime;
using FubuCore.CommandLine;
using FubuCore;

namespace Bottles.Deployment.Commands
{
    public class DeployInput
    {
        [Description("The profile to execute")]
        public string ProfileFlag { get; set; }

        [Description("Path to where the deployment folder is ~/deployment")]
        public string DeploymentFlag { get; set; }

        [FlagAlias("f")]
        public bool ForceFlag { get; set; }

        public string DeploymentRoot()
        {
            return DeploymentFlag ?? ".".ToFullPath();
        }

        public string DeploymentProfile()
        {
            return ProfileFlag.IsNotEmpty()
                       ? ProfileFlag
                       : "default"; //REVIEW: where to put this?
        }
    }

    [CommandDescription("Deploys the given profile")]
    public class DeployCommand : FubuCommand<DeployInput>
    {
        public override bool Execute(DeployInput input)
        {
            var settings = new DeploymentSettings(input.DeploymentRoot())
            {
                UserForced = input.ForceFlag
            };

            var container = DeploymentBootstrapper.Bootstrap(settings);
            var deploymentController = container.GetInstance<IDeploymentController>();
            
            deploymentController.Deploy(new DeploymentOptions(input.DeploymentProfile()));
             
            return true;
        }
    }
}