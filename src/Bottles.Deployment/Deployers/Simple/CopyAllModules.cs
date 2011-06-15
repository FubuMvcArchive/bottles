using System;
using System.Collections.Generic;
using Bottles.Deployment.Runtime;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using System.Linq;

namespace Bottles.Deployment.Deployers.Simple
{
    public class CopyAllModules : IDirective
    {
        public string Destination { get; set; }
    }

    public class CopyAllModulesInitializer : IDeployer<CopyAllModules>
    {
        private readonly DeploymentSettings _settings;
        private readonly IBottleMover _bottleMover;

        public CopyAllModulesInitializer(DeploymentSettings settings, IBottleMover bottleMover)
        {
            _settings = settings;
            _bottleMover = bottleMover;
        }

        public void Execute(CopyAllModules directive, HostManifest host, IPackageLog log)
        {
            var destination = new CopyAllModulesDestination(directive.Destination);
            var references = _settings.AllBottleNames().Select(x => new BottleReference(x));
            _bottleMover.Move(log, destination, references);
        }

        public string GetDescription(CopyAllModules directive)
        {
            return "Copying all module bottles to " + directive.Destination;
        }
    }

    public class CopyAllModulesDestination : IBottleDestination
    {
        private readonly string _destination;

        public CopyAllModulesDestination(string destination)
        {
            _destination = destination;
        }

        public IEnumerable<BottleExplosionRequest> DetermineExplosionRequests(PackageManifest manifest)
        {
            if (manifest.Role == BottleRoles.Module)
            {
                yield return new BottleExplosionRequest(){
                    BottleDirectory = null,
                    BottleName = manifest.Name,
                    DestinationDirectory = _destination
                };
            }
        }
    }
}