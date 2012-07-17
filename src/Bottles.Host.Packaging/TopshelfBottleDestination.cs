using System.Collections.Generic;
using Bottles.Deployment.Runtime.Content;
using FubuCore;

namespace Bottles.Host.Packaging
{
    public class TopshelfBottleDestination : IBottleDestination
    {
        private readonly string _physicalPath;

        public TopshelfBottleDestination(string physicalPath)
        {
            _physicalPath = physicalPath;
        }

        public IEnumerable<BottleExplosionRequest> DetermineExplosionRequests(PackageManifest manifest)
        {
            switch(manifest.Role)
            {
                case BottleRoles.Module:
                    yield return new BottleExplosionRequest
                                 {
                                     BottleDirectory = WellKnownFiles.BinaryFolder,
                                     BottleName = manifest.Name,
                                     DestinationDirectory = _physicalPath.AppendPath(TopshelfBottleLoader.TopshelfPackagesFolder) //is this correct
                                 };
                    break;
                case BottleRoles.Config:
                    yield return new BottleExplosionRequest
                                 {
                                     BottleDirectory = WellKnownFiles.ConfigFolder,
                                     BottleName =  manifest.Name,
                                     DestinationDirectory = _physicalPath.AppendPath(WellKnownFiles.ConfigFolder)
                                 };
                    break;
                case BottleRoles.Binaries:
                case BottleRoles.Application:
                    yield return new BottleExplosionRequest
                                 {
                                     BottleDirectory = WellKnownFiles.BinaryFolder,
                                     BottleName = manifest.Name,
                                     DestinationDirectory = _physicalPath
                                 };
                    break;
                default:
                    yield break;
            }
        }
    }
}