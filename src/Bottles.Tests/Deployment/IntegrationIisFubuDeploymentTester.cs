using System;
using System.IO;
using Bottles.Deployers.Iis;
using Bottles.Deployment;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using Bottles.Zipping;
using FubuCore;
using NUnit.Framework;

namespace Bottles.Tests.Deployment
{
    [TestFixture]
    public class IntegrationIisFubuDeploymentTester
    {
        [Test][Explicit]
        public void DeployHelloWorld()
        {
            IFileSystem fileSystem = new FileSystem();
            var root = Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory);
            var settings = new DeploymentSettings(root.AppendPath("dev","test-profile"));
            IBottleRepository bottles = new BottleRepository(fileSystem, new ZipFileService(fileSystem), settings);

            var initializer = new WebAppOfflineInitializer(fileSystem);
            
            var deployer = new IisWebsiteCreator();
            

            var directive = new Website();
            directive.WebsiteName = "fubu";
            directive.WebsitePhysicalPath = root.AppendPath("dev","test-web");
            directive.VDir = "bob";
            directive.VDirPhysicalPath = root.AppendPath("dev", "test-app");
            directive.AppPool = "fubizzle";
            directive.IdleTimeOut = 30;

            directive.DirectoryBrowsing = Activation.Enable;


            initializer.Execute(directive, new HostManifest("something"), new PackageLog());

            deployer.Create(directive);
        }

        [Test]
        [Explicit]
        public void VerifyCanOverride()
        {
            IFileSystem fileSystem = new FileSystem();
            var root = Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory);
            var settings = new DeploymentSettings(root.AppendPath("dev", "test-profile"));
            IBottleRepository bottles = new BottleRepository(fileSystem, new ZipFileService(fileSystem), settings);

            var initializer = new WebAppOfflineInitializer(fileSystem);

            var deployer = new IisWebsiteCreator();


            var directive = new Website();
            directive.WebsiteName = "fubu";
            directive.WebsitePhysicalPath = root.AppendPath("dev", "test-web");
            directive.VDir = "bob";
            directive.VDirPhysicalPath = root.AppendPath("dev", "test-app");
            directive.AppPool = "fubizzle";

            directive.DirectoryBrowsing = Activation.Enable;


            initializer.Execute(directive, new HostManifest("something"), new PackageLog());

            deployer.Create(directive);

            //override test
            directive.ForceWebsite = true;
            directive.VDirPhysicalPath = root.AppendPath("dev", "test-app2");
            deployer.Create(directive);
        }
    }
}