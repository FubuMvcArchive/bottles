using Bottles.Deployment.Bootstrapping;
using Bottles.Deployment.Diagnostics;
using Bottles.Deployment.Runtime;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class ConfigInput : PlanInput
    {
    }

    
    [CommandDescription("Writes config values to stdout")]
    public class ConfigCommand : FubuCommand<ConfigInput>
    {
        public override bool Execute(ConfigInput input)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);
            var options = input.CreateDeploymentOptions();

            DeploymentBootstrapper.UsingService<IDeploymentController>(settings, x =>
            {
                var plan = x.BuildPlan(options);
                new ConsoleDiagnosticsReporter().WriteReport(options, plan);
            });
            
            return true;
        }
    }
}