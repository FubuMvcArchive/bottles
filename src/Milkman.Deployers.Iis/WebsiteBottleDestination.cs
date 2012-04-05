using System.Collections.Generic;
using Bottles.Deployment.Runtime.Content;
using FubuCore;

namespace Bottles.Deployers.Iis
{
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