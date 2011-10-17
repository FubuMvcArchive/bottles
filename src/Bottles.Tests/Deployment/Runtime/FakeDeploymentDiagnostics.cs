using System;
using Bottles.Deployment;
using Bottles.Deployment.Diagnostics;
using Bottles.Deployment.Parsing;
using Bottles.Diagnostics;

namespace Bottles.Tests.Deployment.Runtime
{
    public class FakeDeploymentDiagnostics : IDeploymentDiagnostics
    {
        public IPackageLog LogFor(object target)
        {
            return null;
        }


        public void LogDeployment(DeploymentPlan plan)
        {
            
        }

        public void LogHost(DeploymentPlan plan, HostManifest hostManifest)
        {
            
        }

        public void LogDirective(HostManifest host, IDirective directive)
        {
            
        }

        public IPackageLog LogAction(HostManifest host, IDirective directive, object action, string description)
        {
            throw new NotImplementedException();
        }

        public void AssertNoFailures()
        {
            throw new NotImplementedException();
        }

        public LoggingSession Session
        {
            get { throw new NotImplementedException(); }
        }


        public void LogExecution(object target, string description, Action continuation)
        {
            continuation();
        }

        public void ForEach(Action<IPackageLog> action)
        {
            
        }
    }
}