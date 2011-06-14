using System;
using Bottles.Deployment;
using Bottles.Deployment.Deployers.Simple;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Bottles.Tests.Deployment.Deployers.Simple
{
    [TestFixture]
    public class ExplodeBottlesInitializerTester : InteractionContext<ExplodeBottlesInitializer>
    {
        private ExplodeBottles theBottle;

        protected override void beforeEach()
        {
            theBottle = new ExplodeBottles(){
                RootDirectory = "root",
                BinDirectory = "bin",
                WebContentDirectory = "web",
                DataDirectory = "data"
            };

            var hostManifest = new HostManifest("something");
            hostManifest.RegisterBottle(new BottleReference("bottle1"));
            hostManifest.RegisterBottle(new BottleReference("bottle2"));

            ClassUnderTest.Execute(theBottle, hostManifest, new PackageLog());
        }

        [Test]
        public void should_delete_and_recreate_the_root_directory()
        {
            MockFor<IFileSystem>().AssertWasCalled(x => x.DeleteDirectory(theBottle.RootDirectory));
            MockFor<IFileSystem>().AssertWasCalled(x => x.CreateDirectory(theBottle.RootDirectory));
        }

        [Test]
        public void explode_the_binaries()
        {
            MockFor<IBottleRepository>().AssertWasCalled(x =>
            {
                x.ExplodeFiles(new BottleExplosionRequest(){
                    BottleDirectory = BottleFiles.BinaryFolder,
                    BottleName = "bottle1",
                    DestinationDirectory = theBottle.RootDirectory.AppendPath(theBottle.BinDirectory)
                });
            });

            MockFor<IBottleRepository>().AssertWasCalled(x =>
            {
                x.ExplodeFiles(new BottleExplosionRequest()
                {
                    BottleDirectory = BottleFiles.BinaryFolder,
                    BottleName = "bottle2",
                    DestinationDirectory = theBottle.RootDirectory.AppendPath(theBottle.BinDirectory)
                });
            });
        }

        [Test]
        public void should_explode_the_web_content()
        {
            MockFor<IBottleRepository>().AssertWasCalled(x =>
            {
                x.ExplodeFiles(new BottleExplosionRequest()
                {
                    BottleDirectory = BottleFiles.WebContentFolder,
                    BottleName = "bottle1",
                    DestinationDirectory = theBottle.RootDirectory.AppendPath(theBottle.WebContentDirectory)
                });
            });

            MockFor<IBottleRepository>().AssertWasCalled(x =>
            {
                x.ExplodeFiles(new BottleExplosionRequest()
                {
                    BottleDirectory = BottleFiles.WebContentFolder,
                    BottleName = "bottle2",
                    DestinationDirectory = theBottle.RootDirectory.AppendPath(theBottle.WebContentDirectory)
                });
            }); 
        }

        [Test]
        public void should_explode_the_data_content()
        {
            MockFor<IBottleRepository>().AssertWasCalled(x =>
            {
                x.ExplodeFiles(new BottleExplosionRequest()
                {
                    BottleDirectory = BottleFiles.DataFolder,
                    BottleName = "bottle1",
                    DestinationDirectory = theBottle.RootDirectory.AppendPath(theBottle.DataDirectory)
                });
            });

            MockFor<IBottleRepository>().AssertWasCalled(x =>
            {
                x.ExplodeFiles(new BottleExplosionRequest()
                {
                    BottleDirectory = BottleFiles.DataFolder,
                    BottleName = "bottle2",
                    DestinationDirectory = theBottle.RootDirectory.AppendPath(theBottle.DataDirectory)
                });
            });
        }
    }
}