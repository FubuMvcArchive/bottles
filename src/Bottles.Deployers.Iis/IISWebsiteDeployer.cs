using Bottles.Deployment;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployers.Iis
{
    public class IISWebsiteDeployer : IDeployer<Website>
    {
        private readonly IisWebsiteCreator _websiteCreator;

        public IISWebsiteDeployer(IisWebsiteCreator websiteCreator)
        {
            _websiteCreator = websiteCreator;
        }

        public void Execute(Website directive, HostManifest host, IPackageLog log)
        {
            directive.VDirPhysicalPath = directive.VDirPhysicalPath.ToFullPath();
            _websiteCreator.Create(directive);
        }
    }
}