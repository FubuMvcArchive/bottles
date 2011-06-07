using System;
using Bottles.Deployment;
using Bottles.Deployment.Configuration;
using Bottles.Deployment.Deployers.Configuration;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Bottles.Tests.Deployment.Deployers.Configuration
{
    [TestFixture]
    public class clean_the_directory_if_overwrite : InteractionContext<ConfigInitializer>
    {
        private CentralConfig theCentralConfig;
        private HostManifest theHost;
        private DeploymentSettings theSettings;

        protected override void beforeEach()
        {
            theCentralConfig = new CentralConfig()
            {
                Directory = "config",
                ProfileFile = "config".AppendPath("Profile.config"),
                EnvironmentFile = "config".AppendPath("EnvironmentSettings.config"),
                CopyBehavior = CopyBehavior.overwrite
            };

            theHost = new HostManifest("host");

            theSettings = new DeploymentSettings()
            {
                Environment = new EnvironmentSettings(),
                Profile = new Profile("something")
            };

            Services.Inject(theSettings);

            ClassUnderTest.Execute(theCentralConfig, theHost, MockFor<IPackageLog>());
        }

        [Test]
        public void should_clean_the_directory()
        {
            MockFor<IFileSystem>().AssertWasCalled(x => x.CleanDirectory(theCentralConfig.Directory));
        }
    }

    [TestFixture]
    public class do_not_clean_the_directory_if_preserve : InteractionContext<ConfigInitializer>
    {
        private CentralConfig theCentralConfig;
        private HostManifest theHost;
        private DeploymentSettings theSettings;

        protected override void beforeEach()
        {
            theCentralConfig = new CentralConfig()
            {
                Directory = "config",
                ProfileFile = "config".AppendPath("Profile.config"),
                EnvironmentFile = "config".AppendPath("EnvironmentSettings.config"),
                CopyBehavior = CopyBehavior.preserve
            };

            theHost = new HostManifest("host");

            theSettings = new DeploymentSettings()
            {
                Environment = new EnvironmentSettings(),
                Profile = new Profile("something")
            };

            Services.Inject(theSettings);

            ClassUnderTest.Execute(theCentralConfig, theHost, MockFor<IPackageLog>());
        }

        [Test]
        public void should_NOT_clean_the_directory()
        {
            MockFor<IFileSystem>().AssertWasNotCalled(x => x.CleanDirectory(theCentralConfig.Directory));
        }
    }

    [TestFixture]
    public class ConfigInitializerTester : InteractionContext<ConfigInitializer>
    {
        private CentralConfig theCentralConfig;
        private HostManifest theHost;
        private DeploymentSettings theSettings;

        protected override void beforeEach()
        {
            theCentralConfig = new CentralConfig(){
                Directory = "config",
                ProfileFile = "config".AppendPath("Profile.config"),
                EnvironmentFile = "config".AppendPath("EnvironmentSettings.config")
            };

            theHost = new HostManifest("host");
            theHost.RegisterBottle(new BottleReference("A"));
            theHost.RegisterBottle(new BottleReference("B"));
            theHost.RegisterBottle(new BottleReference("C"));

            theSettings = new DeploymentSettings(){
                Environment = new EnvironmentSettings(),
                Profile = new Profile("something")
            };
        
            Services.Inject(theSettings);

            ClassUnderTest.Execute(theCentralConfig, theHost, MockFor<IPackageLog>());
        }

        [Test]
        public void should_have_created_the_directory()
        {
            MockFor<IFileSystem>().AssertWasCalled(x => x.CreateDirectory(theCentralConfig.Directory));
        }

        [Test]
        public void should_explode_all_the_bottles()
        {
            var theBottleRepository = MockFor<IBottleRepository>();
            theBottleRepository.AssertWasCalled(x => x.ExplodeFiles(new BottleExplosionRequest(){
                BottleDirectory = BottleFiles.ConfigFolder,
                BottleName = "A",
                DestinationDirectory = theCentralConfig.Directory
            }));

            theBottleRepository.AssertWasCalled(x => x.ExplodeFiles(new BottleExplosionRequest()
            {
                BottleDirectory = BottleFiles.ConfigFolder,
                BottleName = "B",
                DestinationDirectory = theCentralConfig.Directory
            }));

            theBottleRepository.AssertWasCalled(x => x.ExplodeFiles(new BottleExplosionRequest()
            {
                BottleDirectory = BottleFiles.ConfigFolder,
                BottleName = "C",
                DestinationDirectory = theCentralConfig.Directory
            }));


        }

        [Test]
        public void writes_the_environment_settings()
        {
            var theFilename = theCentralConfig.Directory.AppendPath("EnvironmentSettings.config");
            MockFor<IConfigurationWriter>().AssertWasCalled(x => x.Write(theFilename, theSettings.Environment));
        }

        [Test]
        public void writes_the_profile()
        {
            var theFilename = theCentralConfig.Directory.AppendPath("Profile.config");
            MockFor<IConfigurationWriter>().AssertWasCalled(x => x.Write(theFilename, theSettings.Profile));
        }
    }
}