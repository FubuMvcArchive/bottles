using Bottles.Deployment;
using Bottles.Deployment.Commands;
using Bottles.Deployment.Diagnostics;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime;
using Bottles.Deployment.Runtime.Content;
using Bottles.Deployment.Writing;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;
using System.Linq;

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



    [TestFixture]
    public class bundling_a_profile_with_dependencies
    {
        [SetUp]
        protected void beforeEach()
        {
            var theDestination = "some_other_place";
            new FileSystem().DeleteDirectory(theDestination);

            DeploymentSettings theSettings = new DeploymentSettings("some_place");

            var theOptions = new DeploymentOptions("A");

            var writer = new DeploymentWriter("some_place");
            
            var pa = writer.ProfileFor("A");
            pa.AddRecipe("rA1");
            
            writer.RecipeFor("rA1");

            pa.AddProfileDependency("B");

            var pb = writer.ProfileFor("B");
            pb.AddRecipe("rB1");

            writer.RecipeFor("rB1");

            writer.Flush(FlushOptions.Wipeout);
            

            //call the bundle task

            var b = new BundleCommand();
            b.Execute(new BundleInput()
                      {
                          DeploymentFlag = "some_place",
                          Destination = theDestination,
                          ProfileFlag = "A"
                      });
        }


        [Test]
        public void all_profiles_should_be_present()
        {
            var files = new FileSystem().FindFiles(".".AppendPath("some_other_place","deployment","profiles"), new FileSet(){Include = "*.profile"});
            var names = files.Select(System.IO.Path.GetFileName);

            names.ShouldHaveTheSameElementsAs("A.profile","B.profile");
        }
    }


}