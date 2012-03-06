using System.Linq;
using System.ServiceProcess;
using Bottles.Deployment;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployers.Topshelf
{
    public class StartServiceFinalizer : IFinalizer<TopshelfService>
    {
        public void Execute(TopshelfService directive, HostManifest host, IPackageLog log)
        {
            ServiceController svc = getServiceController(directive);

            if(shouldTryAndStartService(svc))
            {
                svc.Start();
            }
        }

        public string GetDescription(TopshelfService directive)
        {
            if(shouldTryAndStartService(getServiceController(directive)))
            {
                return "Starting service '{0}'".ToFormat(directive.ServiceName);
            }

            return "Service '{0}' appears to be started. Doing nothing.".ToFormat(directive.ServiceName);
        }

        private ServiceController getServiceController(TopshelfService directive)
        {
            var x =  ServiceController.GetServices()
                .Where(s => s.ServiceName == directive.ServiceName)
                .DefaultIfEmpty(null)
                .SingleOrDefault();

            if(x==null)
            {
                throw new DeploymentException("Couldn't find the service '{0}'".ToFormat(directive.ServiceName));
            }

            return x;
        }

        private bool shouldTryAndStartService(ServiceController svc)
        {
            return svc.Status != ServiceControllerStatus.Running;
        }
    }
}