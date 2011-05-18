using System;
using System.Diagnostics;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployment.Diagnostics
{
    public class DeploymentDiagnostics : LoggingSession, IDeploymentDiagnostics
    {
        public void LogHost(HostManifest hostManifest)
        {
            LogObject(hostManifest, "Deploying host from deployment ???");
            LogFor("deploymentname").AddChild(hostManifest);
        }

        public void LogDirective(HostManifest host, IDirective directive)
        {
            LogObject(directive, "Found in '{0}'".ToFormat(host));
            LogFor(host).AddChild(directive);
        }

        public PackageLog LogAction(HostManifest host, IDirective directive, object action)
        {
            LogObject(action, "{0} / {1}".ToFormat(host.Name, directive.GetType().Name));
            LogFor(directive).AddChild(action);
            
            return LogFor(action);
        }

        public LoggingSession Session
        {
            get { return this; }
        }
    }
}