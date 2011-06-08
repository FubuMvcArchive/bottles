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
    public class SingleBottleDeployerTester : InteractionContext<SingleBottleDeployer>
    {
        private SingleBottle theBottle;

        protected override void beforeEach()
        {
            theBottle = new SingleBottle(){
                RootDirectory = "root",
                BinDirectory = "bin",
                BottleName = "bottle1",
                WebContentDirectory = "web"
            };

            ClassUnderTest.Execute(theBottle, new HostManifest("something"), new PackageLog());
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
                    BottleName = theBottle.BottleName,
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
                    BottleName = theBottle.BottleName,
                    DestinationDirectory = theBottle.RootDirectory.AppendPath(theBottle.WebContentDirectory)
                });
            }); 
        }
    }
}