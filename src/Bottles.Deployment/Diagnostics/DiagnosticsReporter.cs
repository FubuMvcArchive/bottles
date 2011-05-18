using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime;

namespace Bottles.Deployment.Diagnostics
{
    public class DiagnosticsReporter : IDiagnosticsReporter
    {
        private readonly IDeploymentDiagnostics _diagnostics;

        public DiagnosticsReporter(IDeploymentDiagnostics diagnostics)
        {
            _diagnostics = diagnostics;
        }

        public void WriteReport(DeploymentOptions options, DeploymentPlan plan)
        {
            var report = new DeploymentReport("Deployment Report");
            report.WriteDeploymentPlan(plan);
            report.WriteLoggingSession(_diagnostics.Session);
        }
    }
}