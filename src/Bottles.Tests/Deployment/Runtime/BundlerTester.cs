using System;
using Bottles.Deployment;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Bottles.Tests.Deployment.Runtime
{
    [TestFixture]
    public class when_bundling_by_destination : InteractionContext<Bundler>
    {
        private DeploymentPlan thePlan;
        private DeploymentOptions theOptions;
        private string theDestination;

        protected override void beforeEach()
        {
            theDestination = "some directory";
            Services.Inject(new DeploymentSettings());
            Services.PartialMockTheClassUnderTest();
            thePlan = DeploymentPlan.Blank();
            theOptions = new DeploymentOptions();

            

            MockFor<IDeploymentController>().Stub(x => x.BuildPlan(theOptions))
                .Return(thePlan);

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