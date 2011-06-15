using System;
using System.IO;
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

        public PackageLog LogAction(HostManifest host, IDirective directive, object action, string description)
        {
            var provenance = "Host {0} / Directive {1}".ToFormat(host.Name, directive.GetType().Name);
            LogObject(action, provenance);
            LogFor(directive).AddChild(action);

            LogWriter.RunningStep("{0} for {1}", description, provenance);

            return LogFor(action);
        }

        public LoggingSession Session
        {
            get { return this; }
        }

        public void AssertNoFailures()
        {
            if (!HasErrors()) return;


            var writer = new StringWriter();
            writer.WriteLine("Package loading and aplication bootstrapping failed");
            writer.WriteLine();
            EachLog((o, log) =>
            {
                if (!log.Success)
                {
                    writer.WriteLine(o.ToString());
                    writer.WriteLine(log.FullTraceText());
                    writer.WriteLine("------------------------------------------------------------------------------------------------");
                }
            });

            throw new ApplicationException(writer.GetStringBuilder().ToString());

        }
    }
}