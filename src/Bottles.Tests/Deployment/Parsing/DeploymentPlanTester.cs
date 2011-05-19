using System.Linq;
using Bottles.Configuration;
using Bottles.Deployment;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime;
using NUnit.Framework;
using FubuTestingSupport;

namespace Bottles.Tests.Deployment.Parsing
{
    [TestFixture]
    public class DeploymentPlanTester
    {
        private EnvironmentSettings theEnvironment;
        private Profile theProfile;
        private Recipe[] theRecipes;

        [SetUp]
        public void SetUp()
        {
            theEnvironment = new EnvironmentSettings();
            theEnvironment.Overrides["Env1"] = "Env1-Val";
            theEnvironment.Overrides["Shared"] = "Shared-Env-Val";

            theProfile = new Profile();
            theProfile.AddRecipe("something");

            theProfile.Overrides["Shared"] = "Shared-Profile-Val";
            theProfile.Overrides["Profile1"] = "Profile1-Val";

            theRecipes = new Recipe[] { new Recipe("something"), };
        }


        [Test]
        public void combine_overrides_and_provenance()
        {
            
            var plan = new DeploymentPlan(new DeploymentOptions(), new DeploymentGraph(){
                Environment = theEnvironment,
                Profile = theProfile,
                Recipes = theRecipes,
                Settings = new DeploymentSettings(){TargetDirectory = "target"}
            });

            plan.OverrideSourcing.ShouldHaveTheSameElementsAs(
                new OverrideSource(){Key = "Env1", Provenance = "Environment", Value = "Env1-Val"},
                new OverrideSource(){Key = "Shared", Provenance = "Profile", Value = "Shared-Profile-Val"},
                new OverrideSource(){Key = "Profile1", Provenance = "Profile", Value = "Profile1-Val"},
                new OverrideSource(){Key = EnvironmentSettings.ROOT, Provenance = "DeploymentSettings", Value = "target"}
                );

        }

        [Test]
        public void set_the_target_directory_if_root_exists_in_environment()
        {
            theEnvironment.Overrides[EnvironmentSettings.ROOT] = "env-root";

            var deploymentSettings = new DeploymentSettings();
            var plan = new DeploymentPlan(new DeploymentOptions(), new DeploymentGraph()
            {
                Environment = theEnvironment,
                Profile = theProfile,
                Recipes = theRecipes,
                Settings = deploymentSettings
            });

            deploymentSettings.TargetDirectory.ShouldEqual("env-root");
        }

        [Test]
        public void set_the_target_directory_if_root_exists_in_environment_and_profile_THE_PROFILE_SHOULD_WIN()
        {
            theEnvironment.Overrides[EnvironmentSettings.ROOT] = "env-root";
            theProfile.Overrides[EnvironmentSettings.ROOT] = "profile-root";

            var deploymentSettings = new DeploymentSettings();
            var plan = new DeploymentPlan(new DeploymentOptions(), new DeploymentGraph()
            {
                Environment = theEnvironment,
                Profile = theProfile,
                Recipes = theRecipes,
                Settings = deploymentSettings
            });

            deploymentSettings.TargetDirectory.ShouldEqual("profile-root");
        }

        [Test]
        public void set_the_root_from_deployment_settings_if_root_is_in_neither_environment_or_profile()
        {
            var deploymentSettings = new DeploymentSettings(){
                TargetDirectory = "settings-target"
            };

            var plan = new DeploymentPlan(new DeploymentOptions(), new DeploymentGraph()
            {
                Environment = theEnvironment,
                Profile = theProfile,
                Recipes = theRecipes,
                Settings = deploymentSettings
            });

            plan.Substitutions[EnvironmentSettings.ROOT].ShouldEqual("settings-target");

            var sourcing = plan.OverrideSourcing.Single(x => x.Key == EnvironmentSettings.ROOT);
            sourcing.Value.ShouldEqual("settings-target");
            sourcing.Provenance.ShouldEqual("DeploymentSettings");
        
        }
    }
}