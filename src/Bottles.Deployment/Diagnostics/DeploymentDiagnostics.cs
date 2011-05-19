using System;
using System.Diagnostics;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;
using FubuCore.CommandLine;

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
            var provenance = "{0} / {1}".ToFormat(host.Name, directive.GetType().Name);
            LogObject(action, provenance);
            LogFor(directive).AddChild(action);
            

            ConsoleWriter.Line();
            ConsoleWriter.PrintHorizontalLine();
            ConsoleWriter.WriteWithIndent(ConsoleColor.White, 4, "Running {0} for {1}".ToFormat(action, provenance));
            

            return LogFor(action);
        }

        public LoggingSession Session
        {
            get { return this; }
        }
    }
}