using System;
using Bottles.Deployment;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime;
using Bottles.Deployment.Runtime.Content;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Bottles.Tests.Deployment.Runtime
{
    [TestFixture]
    public class when_exploding_deployer_bottles : InteractionContext<Bundler>
    {
        private DeploymentPlan thePlan;
        private DeploymentOptions theOptions;
        private string theDestination;
        private DeploymentSettings theSettings;
        private string[] theDeployerBottles;

        protected override void beforeEach()
        {
            theDestination = "some directory";

            theSettings = MockFor<DeploymentSettings>();

            Services.Inject<DeploymentSettings>(theSettings);

            thePlan = DeploymentPlan.Blank();
            theOptions = new DeploymentOptions();

            theDeployerBottles = new string[] { "a", "b", "c" };
            theSettings.Expect(x => x.DeployerBottleNames()).Return(theDeployerBottles);

            MockFor<IDeploymentController>().Stub(x => x.BuildPlan(theOptions))
                .Return(thePlan);

            ClassUnderTest.ExplodeDeployerBottles(theDestination);
        }

        [Test]
        public void should_explode_all_the_deployer_bottles()
        {
            MockFor<IBottleRepository>().AssertWasCalled(x =>
            {
                x.ExplodeFiles(new BottleExplosionRequest()
                {
                    BottleDirectory = BottleFiles.BinaryFolder,
                    BottleName = "a",
                    DestinationDirectory = theDestination
                });
            });

            MockFor<IBottleRepository>().AssertWasCalled(x =>
            {
                x.ExplodeFiles(new BottleExplosionRequest()
                {
                    BottleDirectory = BottleFiles.BinaryFolder,
                    BottleName = "b",
                    DestinationDirectory = theDestination
                });
            });

            MockFor<IBottleRepository>().AssertWasCalled(x =>
            {
                x.ExplodeFiles(new BottleExplosionRequest()
                {
                    BottleDirectory = BottleFiles.BinaryFolder,
                    BottleName = "c",
                    DestinationDirectory = theDestination
                });
            });
        }  
    }


    [TestFixture]
    public class when_bundling_by_destination : InteractionContext<Bundler>
    {
        private DeploymentPlan thePlan;
        private DeploymentOptions theOptions;
        private string theDestination;
        private DeploymentSettings theSettings;
        private string[] theDeployerBottles;

        protected override void beforeEach()
        {
            theDestination = "some directory";

            theSettings = MockFor<DeploymentSettings>();

            Services.Inject<DeploymentSettings>(theSettings);
            
            thePlan = DeploymentPlan.Blank();
            theOptions = new DeploymentOptions();

            theDeployerBottles = new string[]{"a", "b", "c"};
            theSettings.Expect(x => x.DeployerBottleNames()).Return(theDeployerBottles);

            MockFor<IDeploymentController>().Stub(x => x.BuildPlan(theOptions))
                .Return(thePlan);

            Services.PartialMockTheClassUnderTest();

            ClassUnderTest.Expect(x => x.CreateBundle(theDestination, thePlan));

            ClassUnderTest.CreateBundle(theDestination, theOptions);
        }



        [Test]
        public void should_build_the_bundle_with_the_plan_determined_by_the_controller()
        {
            ClassUnderTest.VerifyAllExpectations();
        }
    }

    
}