using Bottles.Deployment;
using Bottles.Deployment.Runtime;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployers.Iis
{
    public class IisWebsiteDeployer : IDeployer<Website>
    {
        private readonly IisWebsiteCreator _websiteCreator;
        private readonly IBottleMover _bottleMover;

        public IisWebsiteDeployer(IisWebsiteCreator websiteCreator, IBottleMover bottleMover)
        {
            _websiteCreator = websiteCreator;
            _bottleMover = bottleMover;
        }

        public void Execute(Website directive, HostManifest host, IPackageLog log)
        {
            directive.VDirPhysicalPath = directive.VDirPhysicalPath.ToFullPath();

            cleanUpTargetFolderIfRequested(directive, log);

            _websiteCreator.Create(directive);

            var destination = new WebsiteBottleDestination(directive.VDirPhysicalPath);
            _bottleMover.Move(log, destination, host.BottleReferences);
        }

        private static void cleanUpTargetFolderIfRequested(Website directive, IPackageLog log)
        {
            if (!directive.Clean) return;


            log.Trace("Cleaning up target directory '{0}'", directive.VDirPhysicalPath);
            new FileSystem().CleanDirectory(directive.VDirPhysicalPath);
        }

        public string GetDescription(Website directive)
        {
            return "Creating a new IIS website " + directive;
        }
    }
}