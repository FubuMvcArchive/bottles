using System;
using System.ComponentModel;
using Bottles.Deployment.Bootstrapping;
using Bottles.Deployment.Runtime;
using FubuCore.CommandLine;
using System.Collections.Generic;
using System.Linq;

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

        [Description("Override any profile settings in form arg1:value1 arg2:value2 arg3:value3")]
        [RequiredUsage("overrides")]
        public string[] Overrides { get; set; }

        public DeploymentOptions CreateDeploymentOptions()
        {
            var options = new DeploymentOptions(ProfileFlag){
                ReportName = ReportFlag
            };

            if (Overrides != null)
            {
                Overrides.Select(x => x.Split(':')).Each(parts =>
                {
                    options.Overrides[parts[0]] = parts[1];
                });
            }

            return options;
        }
    }

    [CommandDescription("Deploys the given profile")]
    [Usage("overrides", "Deploy with property overrides")]
    [Usage("default", "Deploy with only the environment settings in the deployment folder")]
    public class DeployCommand : FubuCommand<DeployInput>
    {
        public override bool Execute(DeployInput input)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);

            var options = input.CreateDeploymentOptions();

            DeploymentBootstrapper.UsingService<IDeploymentController>(settings, x => x.Deploy(options));
             
            // TODO -- need to blow up / fail if there were any errors detected

            return true;
        }
    }
}