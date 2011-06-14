using System.Linq;
using Bottles.Deployment.Deployers.Simple;
using NUnit.Framework;
using FubuTestingSupport;

namespace Bottles.Tests.Deployment.Deployers.Simple
{
    [TestFixture]
    public class CopyAllModulesInitializerTester
    {
        
    }

    [TestFixture]
    public class CopyAllModulesDestinationTester
    {
        [Test]
        public void destermine_explosion_request_does_nothing_with_manifest_that_is_not_module()
        {
            var destination = new CopyAllModulesDestination("something");

            destination.DetermineExplosionRequests(new PackageManifest(){Role = BottleRoles.Application}).Any().ShouldBeFalse();
            destination.DetermineExplosionRequests(new PackageManifest(){Role = BottleRoles.Binaries}).Any().ShouldBeFalse();
            destination.DetermineExplosionRequests(new PackageManifest(){Role = BottleRoles.Config}).Any().ShouldBeFalse();
            destination.DetermineExplosionRequests(new PackageManifest(){Role = BottleRoles.Data}).Any().ShouldBeFalse();
        }

        [Test]
        public void creates_a_single_explosion_request_for_a_module()
        {
            var manifest = new PackageManifest(){
                Name = "the manifest",
                Role = BottleRoles.Module
            };

            var destination = new CopyAllModulesDestination("something");
            var request = destination.DetermineExplosionRequests(manifest).Single();

            // copies the bottle as is
            request.BottleDirectory.ShouldBeNull();

            request.BottleName.ShouldEqual(manifest.Name);
            request.DestinationDirectory.ShouldEqual("something");
        }
    }
}