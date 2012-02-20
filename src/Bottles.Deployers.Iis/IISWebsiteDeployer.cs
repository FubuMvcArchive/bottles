using System.Collections.Generic;
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

            _websiteCreator.Create(directive);

            var destination = new WebsiteBottleDestination(directive.VDirPhysicalPath);
            _bottleMover.Move(log, destination, host.BottleReferences);
        }

        public string GetDescription(Website directive)
        {
            return "Creating a new IIS website " + directive;
        }
    }

    public class WebsiteBottleDestination : IBottleDestination
    {
        private readonly string _physicalPath;

        public WebsiteBottleDestination(string physicalPath)
        {
            _physicalPath = physicalPath;            
        }

        public virtual IEnumerable<BottleExplosionRequest> DetermineExplosionRequests(PackageManifest manifest)
        {
            switch (manifest.Role)
            {
                case BottleRoles.Binaries:
                    yield return new BottleExplosionRequest
                    {
                        BottleDirectory = BottleFiles.BinaryFolder,
                        BottleName = manifest.Name,
                        DestinationDirectory = FileSystem.Combine(_physicalPath, BottleFiles.BinaryFolder)
                    };
                    break;

                case BottleRoles.Config:
                    yield return new BottleExplosionRequest()
                    {
                        BottleDirectory = BottleFiles.ConfigFolder,
                        BottleName = manifest.Name,
                        DestinationDirectory = FileSystem.Combine(_physicalPath, BottleFiles.ConfigFolder)
                    };
                    break;

                case BottleRoles.Module:                    
                    yield return new BottleExplosionRequest
                    {
                        BottleDirectory = BottleFiles.BinaryFolder,
                        BottleName = manifest.Name,
                        DestinationDirectory = _physicalPath.AppendPath(BottleFiles.BinaryFolder)
                    };
                    break;

                case BottleRoles.Application:
                    yield return new BottleExplosionRequest
                    {
                        BottleName = manifest.Name,
                        BottleDirectory = BottleFiles.BinaryFolder,
                        DestinationDirectory = FileSystem.Combine(_physicalPath, BottleFiles.BinaryFolder)
                    };

                    yield return new BottleExplosionRequest
                    {
                        BottleName = manifest.Name,
                        BottleDirectory = BottleFiles.WebContentFolder,
                        DestinationDirectory = _physicalPath
                    };

                    break;

                default:
                    yield break;
            }
        }
    }    
}