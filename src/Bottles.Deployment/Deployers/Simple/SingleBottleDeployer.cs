using Bottles.Deployment.Runtime;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployment.Deployers.Simple
{
    public class SingleBottleDeployer : IDeployer<SingleBottle>
    {
        private readonly IFileSystem _fileSystem;
        private readonly IBottleRepository _bottles;

        public SingleBottleDeployer(IFileSystem fileSystem, IBottleRepository bottles)
        {
            _fileSystem = fileSystem;
            _bottles = bottles;
        }

        public void Execute(SingleBottle directive, HostManifest host, IPackageLog log)
        {
            _fileSystem.DeleteDirectory(directive.RootDirectory);
            _fileSystem.CreateDirectory(directive.RootDirectory);

            _bottles.ExplodeFiles(new BottleExplosionRequest(log){
                BottleDirectory = BottleFiles.BinaryFolder,
                DestinationDirectory = directive.RootDirectory.AppendPath(directive.BinDirectory ?? string.Empty),
                BottleName = directive.BottleName
            });

            _bottles.ExplodeFiles(new BottleExplosionRequest(log)
                                  {
                                      BottleDirectory = BottleFiles.WebContentFolder,
                                      DestinationDirectory = directive.RootDirectory.AppendPath(directive.WebContentDirectory ?? string.Empty),
                                      BottleName = directive.BottleName
                                  });
        }

    }
}