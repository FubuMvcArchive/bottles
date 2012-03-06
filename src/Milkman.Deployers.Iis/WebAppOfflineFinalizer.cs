using Bottles.Deployment;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;
using Microsoft.Web.Administration;

namespace Bottles.Deployers.Iis
{
    public class WebAppOfflineFinalizer : IFinalizer<Website>
    {
        private readonly IFileSystem _fileSystem;

        public WebAppOfflineFinalizer(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void Execute(Website directive, HostManifest host, IPackageLog log)
        {
            _fileSystem.DeleteFile(FileSystem.Combine(directive.VDirPhysicalPath, "app_offline.htm"));

            restartPools(directive, log);
        }

        private void restartPools(Website directive, IPackageLog log)
        {
            using(var sm = new ServerManager())
            {
                foreach(var pool in sm.ApplicationPools)
                {
                    if (!pool.Name.EqualsIgnoreCase(directive.AppPool)) continue;
                    
                    if(pool.State == ObjectState.Stopped)
                    {
                        log.Trace("Starting pool '{0}'", pool.Name);

                        pool.Start();
                    }
                }
            }
        }


        public string GetDescription(Website directive)
        {
            return "Removing the app_offline.htm file from " + directive.VDirPhysicalPath;
        }
    }
}