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
    public class ConfigInitializerTester : InteractionContext<ConfigInitializer>
    {
        private CentralConfig theCentralConfig;
        private HostManifest theHost;
        private DeploymentSettings theSettings;

        protected override void beforeEach()
        {
            theCentralConfig = new CentralConfig(){
                Directory = "config"
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
            theBottleRepository.AssertWasCalled(x => x.ExplodeTo("A", theCentralConfig.Directory));
            theBottleRepository.AssertWasCalled(x => x.ExplodeTo("B", theCentralConfig.Directory));
            theBottleRepository.AssertWasCalled(x => x.ExplodeTo("C", theCentralConfig.Directory));
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