using Bottles.Deployment;
using Bottles.Deployment.Directives;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;

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
            _websiteCreator.Create(directive);
        }
    }
}