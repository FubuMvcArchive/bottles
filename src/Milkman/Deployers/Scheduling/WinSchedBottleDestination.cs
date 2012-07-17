using System.Collections.Generic;
using Bottles.Deployment.Runtime.Content;
using FubuCore;

namespace Bottles.Deployment.Deployers.Scheduling
{
    public class WinSchedBottleDestination : IBottleDestination
    {
        private string _physicalPath;

        public WinSchedBottleDestination(string physicalPath)
        {
            _physicalPath = physicalPath;
        }

        public IEnumerable<BottleExplosionRequest> DetermineExplosionRequests(PackageManifest manifest)
        {
            switch (manifest.Role)
            {
                case BottleRoles.Module:
                    yield return new BottleExplosionRequest
                                 {
                                     BottleDirectory = SchedTaskBottleFacility.PackagesFolder,
                                     BottleName = manifest.Name,
                                     DestinationDirectory = _physicalPath.AppendPath(WellKnownFiles.PackagesFolder) //is this correct
                                 };
                    break;
                case BottleRoles.Config:
                    yield return new BottleExplosionRequest
                                 {
                                     BottleDirectory = WellKnownFiles.ConfigFolder,
                                     BottleName = manifest.Name,
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