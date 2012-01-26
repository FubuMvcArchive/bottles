using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
            var x = _runner.Run(psi, new TimeSpan(0, 0, 0, 20));
            log.Trace("Exited with {0}", x.ExitCode);
            log.Trace("Process output:{0}{1}", System.Environment.NewLine, x.OutputText);
            x.AssertMandatorySuccess();
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