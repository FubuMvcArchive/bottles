using System;
using System.Linq;
using System.ServiceProcess;
using Bottles.Deployment;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;
using TimeoutException = System.TimeoutException;

namespace Bottles.Deployers.Topshelf
{
    public class TopshelfServiceStopper : IInitializer<TopshelfService>
    {
        public void Execute(TopshelfService directive, HostManifest host, IPackageLog log)
        {
            stopServiceIfItExists(directive, log);
        }

        public string GetDescription(TopshelfService directive)
        {
            return "Stops service '{0}'".ToFormat(directive.ServiceName);
        }

        private void stopServiceIfItExists(TopshelfService directive, IPackageLog log)
        {
            var service = ServiceController.GetServices()
                .DefaultIfEmpty(null)
                .SingleOrDefault(sn => sn.ServiceName.Equals(directive.ServiceName));

            if (serviceDoesntNeedStopping(service))
            {
                return;
            }

            log.Trace("Found service '{0}' and trying to stop it", directive.ServiceName);


            stopService(service, log);
        }

        private bool serviceDoesntNeedStopping(ServiceController service)
        {
            if (service == null)
            {
                return true;
            }
            return (service.Status == ServiceControllerStatus.Stopped || service.Status == ServiceControllerStatus.StopPending);
        }

        private void stopService(ServiceController service, IPackageLog log)
        {
            if (!service.CanStop)
            {
                log.MarkFailure("Not allowed to stop the service '{0}'".ToFormat(service.ServiceName));
                return;
            }

            try
            {
                service.Stop();

                service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 0, 30)); //waits 30 seconds
            }
            catch (TimeoutException ex)
            {
                log.MarkFailure(
                    "Unable to stop service '{0}' using conventional stop after 30 seconds".ToFormat(
                        service.ServiceName));
            }
            catch (InvalidOperationException ioex)
            {
                //Why is this check here - removing throw - dru 5/23/2012
                if (!ioex.Message.Contains("does not exist"))
                {
                    //throw;
                }
                log.Trace("Service is not there anymore. Carry on.");
            }
            catch (Exception e)
            {
                log.Trace("Encountered an exception while stopping the service '{0}': {1}", service.ServiceName, e);
            }
        }
    }
}