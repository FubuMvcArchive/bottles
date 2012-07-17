using Bottles.Deployment.Parsing;
using Bottles.Diagnostics;

namespace Bottles.Deployment.Diagnostics
{
    public interface IDeploymentDiagnostics
    {        
        IBottleLog LogFor(object target);
        void LogDeployment(DeploymentPlan plan);
        void LogHost(DeploymentPlan plan, HostManifest hostManifest);



        //used
        void LogDirective(HostManifest host, IDirective directive);
        IBottleLog LogAction(HostManifest host, IDirective directive, object action, string description);
        LoggingSession Session { get; }
        void AssertNoFailures();
    }
}