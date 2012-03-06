using Bottles.Deployment;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;
using Microsoft.Web.Administration;

namespace Bottles.Deployers.Iis
{
    public class WebAppOfflineInitializer : IInitializer<Website>
    {
        private readonly IFileSystem _fileSystem;

        public WebAppOfflineInitializer(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void Execute(Website directive, HostManifest host, IPackageLog log)
        {
            // Just in case these directories do not already exist
            _fileSystem.CreateDirectory(directive.WebsitePhysicalPath);
            _fileSystem.CreateDirectory(directive.VDirPhysicalPath);


            stopPools(directive, log);


            var appOfflineFile = FileSystem.Combine(directive.VDirPhysicalPath, "app_offline.htm");

            // TODO -- make this nicer
            log.Trace("Writing the application offline file to " + appOfflineFile);
            _fileSystem.WriteStringToFile(appOfflineFile,
                                          "&lt;html&gt;&lt;body&gt;Application is being rebuilt&lt;/body&gt;&lt;/html&gt;");
        }

        private static void stopPools(Website directive, IPackageLog log)
        {
            using (var sm = new ServerManager())
            {
                foreach (var pool in sm.ApplicationPools)
                {
                    if (!pool.Name.EqualsIgnoreCase(directive.AppPool)) continue;

                    if (pool.State == ObjectState.Started)
                    {
                        log.Trace("Stopping pool '{0}'", pool.Name);
                        pool.Stop();
                    }
                }

                sm.CommitChanges();
            }
        }

        public string GetDescription(Website directive)
        {
            return "Writing the app_offline_htm file to " + directive.VDirPhysicalPath;
        }
    }
}