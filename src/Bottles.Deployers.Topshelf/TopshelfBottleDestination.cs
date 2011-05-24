using System;
using System.Collections.Generic;
using Bottles.Deployment.Runtime.Content;
using FubuCore;

namespace Bottles.Deployers.Topshelf
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
                case BottleRoles.Binaries:
                case BottleRoles.Module:
                    yield return new BottleExplosionRequest
                                 {
                                     BottleDirectory = BottleFiles.PackagesFolder,
                                     BottleName = manifest.Name,
                                     DestinationDirectory = _physicalPath.AppendPath("svc",BottleFiles.PackagesFolder) //is this correct
                                 };
                    break;
                case BottleRoles.Config:
                    yield return new BottleExplosionRequest
                                 {
                                     BottleDirectory = BottleFiles.ConfigFolder,
                                     BottleName =  manifest.Name,
                                     DestinationDirectory = _physicalPath.AppendPath(BottleFiles.ConfigFolder)
                                 };
                    break;
                case BottleRoles.Application:
                    yield return new BottleExplosionRequest
                                 {
                                     BottleDirectory = BottleFiles.BinaryFolder,
                                     BottleName = manifest.Name,
                                     DestinationDirectory = _physicalPath.AppendPath("svc")
                                 };
                    break;
                default:
                    yield break;
            }
        }
    }
}