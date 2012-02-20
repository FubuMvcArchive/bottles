using Bottles.Deployment.Commands;
using Bottles.Deployment.Runtime;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;

namespace Bottles.Tests.Deployment.Commands
{
    [TestFixture]
    public class when_building_a_deoployment_options
    {
        private DeployInput theInput;
        private DeploymentOptions theOptions;

        [SetUp]
        public void SetUp()
        {
            theInput = new DeployInput()
                       {
                           ProfileFlag = "milkman"
                       };
            theInput.OverrideFlag = "a:1;b:2;c:3";

            theOptions = theInput.CreateDeploymentOptions();
        }

        [Test]
        public void should_have_set_the_profile()
        {
            theOptions.ProfileName.ShouldEqual(theInput.ProfileFlag);
        }

        [Test]
        public void should_set_the_report_name()
        {
            theOptions.ReportName.ShouldEqual(theInput.ReportFlag);
        }

        [Test]
        public void should_set_any_overrides_by_parsing_values()
        {
            theOptions.Overrides.GetAllKeys().ShouldHaveTheSameElementsAs("a", "b", "c");

            theOptions.Overrides["a"].ShouldEqual("1");
            theOptions.Overrides["b"].ShouldEqual("2");
            theOptions.Overrides["c"].ShouldEqual("3");
        }

        [Test]
        public void do_not_set_any_overrides_if_none_on_input()
        {
            var input = new DeployInput{
                OverrideFlag = null
            };

            input.CreateDeploymentOptions().Overrides.GetAllKeys().Any().ShouldBeFalse();
        }

        [Test]
        public void no_imported_folders()
        {
            new DeployInput().CreateDeploymentOptions().ImportedFolders.Any().ShouldBeFalse();
        }

        [Test]
        public void imported_folders()
        {
            var input = new DeployInput(){
                ImportedFolders = new string[]{"a", "b", "c"}
            };

            input.CreateDeploymentOptions().ImportedFolders.ShouldHaveTheSameElementsAs("a", "b", "c");
        }
    }
}