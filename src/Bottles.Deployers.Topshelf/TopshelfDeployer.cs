using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Bottles.Deployment;
using Bottles.Deployment.Runtime;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using Bottles.Host.Packaging;
using Bottles.Services;
using FubuCore;

namespace Bottles.Deployers.Topshelf
{
    //assumes its on the same server
    public class TopshelfDeployer : IDeployer<TopshelfService>
    {
        public static readonly string BOTTLE_NAME = "topshelf-deployers";
        
        private readonly IProcessRunner _runner;
        private readonly IBottleMover _bottelMover;


        public TopshelfDeployer(IProcessRunner runner, IBottleMover bottelMover)
        {
            _bottelMover = bottelMover;
            _runner = runner;
        }

        public void Execute(TopshelfService directive, HostManifest host, IPackageLog log)
        {
            stopServiceIfItExists(directive, log);
            var destination = new TopshelfBottleDestination(directive.InstallLocation);
            var bottleReferences = new List<BottleReference>(host.BottleReferences){
                new BottleReference(BOTTLE_NAME)
            };

            _bottelMover.Move(log, destination, bottleReferences);

            var cfgFile  = directive.InstallLocation.AppendPath(ServiceInfo.FILE);

            new FileSystem().WriteToFlatFile(cfgFile, writer =>
            {
                writer.WriteProperty("Name",host.Name);
                writer.WriteProperty("Bootstrapper",directive.Bootstrapper);
            });

            var args = buildInstallArgs(directive);
            var psi = new ProcessStartInfo("Bottles.Host.exe"){
                Arguments = args,
                WorkingDirectory = directive.InstallLocation
            };

            log.Trace("Topshelf Install: {0}", buildInstallArgsForDisplay(directive));
            _runner.Run(psi);
        }

        private void stopServiceIfItExists(TopshelfService directive, IPackageLog log)
        {
            var service = ServiceController.GetServices()
                .DefaultIfEmpty(null)
                .SingleOrDefault(sn=>sn.ServiceName.Equals(directive.ServiceName));
            
            if(service != null && service.CanStop)
            {
                log.Trace("Found service '{0}' and stopping", directive.ServiceName);
                service.Stop();
            }
        }

        private static string buildInstallArgs(TopshelfService directive)
        {
            var sb = new StringBuilder();
            sb.Append("install");

            //these first two allow for spaces
            directive.DisplayName.IsNotEmpty(s => sb.AppendFormat(" -displayname \"{0}\"", s));
            directive.Description.IsNotEmpty(s => sb.AppendFormat(" -description \"{0}\"", s));
            directive.ServiceName.IsNotEmpty(s => sb.AppendFormat(" -servicename:{0}", s));


            directive.Username.IsNotEmpty(s => sb.AppendFormat(" -username:{0}", s));
            directive.Password.IsNotEmpty(s => sb.AppendFormat(" -password:{0}", s));


            return sb.ToString();
        }

        private static string buildInstallArgsForDisplay(TopshelfService directive)
        {
            var sb = new StringBuilder();
            sb.Append("install");

            //these first two allow for spaces
            directive.DisplayName.IsNotEmpty(s => sb.AppendFormat(" -displayname \"{0}\"", s));
            directive.Description.IsNotEmpty(s => sb.AppendFormat(" -description \"{0}\"", s));
            directive.ServiceName.IsNotEmpty(s => sb.AppendFormat(" -servicename:{0}", s));


            directive.Username.IsNotEmpty(s => sb.AppendFormat(" -username:{0}", s));
            directive.Password.IsNotEmpty(s => sb.AppendFormat(" -password:*****"));


            return sb.ToString();
        }

        public string GetDescription(TopshelfService directive)
        {
            return "Creating TopShelf service " + directive;
        }
    }
}