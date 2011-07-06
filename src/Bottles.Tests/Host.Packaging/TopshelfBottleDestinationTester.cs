using System.Linq;
using Bottles.Host.Packaging;
using NUnit.Framework;
using FubuTestingSupport;
using FubuCore;

namespace Bottles.Tests.Host.Packaging
{
    [TestFixture]
    public class TopshelfBottleDestinationTester
    {
        [Test]
        public void ModulePackage()
        {
            var dest = new TopshelfBottleDestination("bob");
            var mani = new PackageManifest()
                       {
                           Name = "hi",
                           Role = BottleRoles.Module
                       };
            var requests = dest.DetermineExplosionRequests(mani);
            requests.Count().ShouldEqual(1);

            var req = requests.Single();

            req.BottleDirectory.ShouldEqual(BottleFiles.BinaryFolder);
            req.BottleName = mani.Name;
            req.DestinationDirectory = "bob".AppendPath(TopshelfPackageLoader.TopshelfPackagesFolder);
        }

        [Test]
        public void BinariesPackage()
        {
            var dest = new TopshelfBottleDestination("bob");
            var mani = new PackageManifest()
            {
                Name = "hi",
                Role = BottleRoles.Binaries
            };
            var requests = dest.DetermineExplosionRequests(mani);
            requests.Count().ShouldEqual(1);

            var req = requests.Single();

            req.BottleDirectory.ShouldEqual("bin");
            req.BottleName = mani.Name;
            req.DestinationDirectory = "bob".AppendPath(BottleFiles.PackagesFolder);
        }

        [Test]
        public void ConfigPackage()
        {
            var dest = new TopshelfBottleDestination("bob");
            var mani = new PackageManifest()
            {
                Name = "hi",
                Role = BottleRoles.Config
            };
            var requests = dest.DetermineExplosionRequests(mani);
            requests.Count().ShouldEqual(1);

            var req = requests.Single();

            req.BottleDirectory.ShouldEqual(BottleFiles.ConfigFolder);
            req.BottleName = mani.Name;
            req.DestinationDirectory = "bob".AppendPath(BottleFiles.ConfigFolder);
        }

        [Test]
        public void ApplicationPackage()
        {
            var dest = new TopshelfBottleDestination("bob");
            var mani = new PackageManifest()
            {
                Name = "hi",
                Role = BottleRoles.Application
            };
            var requests = dest.DetermineExplosionRequests(mani);
            requests.Count().ShouldEqual(1);

            var req = requests.Single();

            req.BottleDirectory.ShouldEqual(BottleFiles.BinaryFolder);
            req.BottleName = mani.Name;
            req.DestinationDirectory = "bob".AppendPath(BottleFiles.PackagesFolder);
        }
    }
}