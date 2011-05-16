using System.ComponentModel;
using Bottles.Deployment.Bootstrapping;
using Bottles.Deployment.Diagnostics;
using Bottles.Deployment.Runtime;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{

    public class ReportInput
    {
        [Description("The profile to execute")]
        public string ProfileFlag { get; set; }

        [Description("Path to where the deployment folder is ~/deployment")]
        public string DeploymentFlag { get; set; }

        [Description("Open the report in the default browser")]
        public bool OpenFlag { get; set; }

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

    [CommandDescription("Generates Report", Name="report")]
    public class ReportCommand : FubuCommand<ReportInput>
    {
        public override bool Execute(ReportInput input)
        {
            var settings = new DeploymentSettings(input.DeploymentRoot());

            var container = DeploymentBootstrapper.Bootstrap(settings);
            var reporter = container.GetInstance<IDiagnosticsReporter>();

            reporter.Report(new DeploymentOptions(input.DeploymentProfile()), "report.html");

            if(input.OpenFlag)
                new FileSystem().LaunchBrowser("report.html");

            return true;
        }
    }

}