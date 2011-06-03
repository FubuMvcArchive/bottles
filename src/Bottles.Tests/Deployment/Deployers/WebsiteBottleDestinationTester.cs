using System.Collections.Generic;
using System.Linq;
using Bottles.Deployers.Iis;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Tests.Deployment.Deployers
{
    [TestFixture]
    public class WebsiteBottleDestinationTester
    {
        private WebsiteBottleDestination theDestination;
        private string theRootFolder;

        [SetUp]
        public void SetUp()
        {
            theRootFolder = "deployedPath";
            theDestination = new WebsiteBottleDestination(theRootFolder);
        }

        [Test]
        public void create_request_for_binaries()
        {
            var request = theDestination.DetermineExplosionRequests(new PackageManifest()
            {
                Role = BottleRoles.Binaries,
                Name = "the bottle name"
            }).Single();

            request.BottleDirectory.ShouldEqual(BottleFiles.BinaryFolder);
            request.DestinationDirectory.ShouldEqual(FileSystem.Combine(theRootFolder, BottleFiles.BinaryFolder));
            request.BottleName.ShouldEqual("the bottle name");
        }

        [Test]
        public void create_request_for_config()
        {
            var request = theDestination.DetermineExplosionRequests(new PackageManifest()
            {
                Role = BottleRoles.Config,
                Name = "the bottle name"
            }).Single();

            request.BottleDirectory.ShouldEqual(BottleFiles.ConfigFolder);
            request.DestinationDirectory.ShouldEqual(FileSystem.Combine(theRootFolder, BottleFiles.ConfigFolder));
            request.BottleName.ShouldEqual("the bottle name");
        }

        [Test]
        public void create_request_for_module()
        {
            var requests = theDestination.DetermineExplosionRequests(new PackageManifest()
            {
                Role = BottleRoles.Module,
                Name = "the bottle name"
            });

            var secondRequet = requests.First();
            secondRequet.BottleDirectory.ShouldEqual(BottleFiles.BinaryFolder);
            secondRequet.DestinationDirectory.ShouldEqual(theRootFolder.AppendPath(BottleFiles.BinaryFolder));
        }

        [Test]
        public void create_requests_for_module()
        {
            var requests = theDestination.DetermineExplosionRequests(new PackageManifest()
            {
                Role = BottleRoles.Application,
                Name = "the bottle name"
            });

            var expected = new List<BottleExplosionRequest>{
                new BottleExplosionRequest(new PackageLog()){BottleName = "the bottle name", BottleDirectory = BottleFiles.BinaryFolder, DestinationDirectory = FileSystem.Combine(theRootFolder, BottleFiles.BinaryFolder)},
                new BottleExplosionRequest(new PackageLog()){BottleName = "the bottle name", BottleDirectory = BottleFiles.WebContentFolder, DestinationDirectory = theRootFolder},
            };

            requests.ShouldHaveTheSameElementsAs(expected);
        }
    }
}