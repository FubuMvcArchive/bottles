using System.Linq;
using Bottles.Deployment;
using Bottles.Deployment.Configuration;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime;
using FubuCore.Configuration;
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
            theEnvironment.Data["Env1"] = "Env1-Val";
            theEnvironment.Data["Shared"] = "Shared-Env-Val";

            theProfile = new Profile("profile1");
            theProfile.AddRecipe("something");

            theProfile.Data["Shared"] = "Shared-Profile-Val";
            theProfile.Data["Profile1"] = "Profile1-Val";

            theRecipes = new Recipe[] { new Recipe("something"), };
        }

        [Test]
        public void puts_any_overrides_onto_profile()
        {
            var options = new DeploymentOptions();
            options.Overrides["Shared"] = "override-val";
            options.Overrides["Profile1"] = "override-profile1-val";

            var plan = new DeploymentPlan(options, new DeploymentGraph()
            {
                Environment = theEnvironment,
                Profile = theProfile,
                Recipes = theRecipes,
                Settings = new DeploymentSettings() { TargetDirectory = "target" }
            });

            plan.GetSubstitutionDiagnosticReport().Single(x => x.Key == "Shared")
                .Value.ShouldEqual("override-val");

            plan.GetSubstitutionDiagnosticReport().Single(x => x.Key == "Profile1")
                .Value.ShouldEqual("override-profile1-val");
        }

        [Test]
        public void sets_itself_on_deployment_settings()
        {
            var plan = new DeploymentPlan(new DeploymentOptions(), new DeploymentGraph()
            {
                Environment = theEnvironment,
                Profile = theProfile,
                Recipes = theRecipes,
                Settings = new DeploymentSettings() { TargetDirectory = "target" }
            });

            plan.Settings.Plan.ShouldBeTheSameAs(plan);
        }

        [Test]
        public void sets_environment_and_profile_on_deployment_settings()
        {
            var plan = new DeploymentPlan(new DeploymentOptions(), new DeploymentGraph()
            {
                Environment = theEnvironment,
                Profile = theProfile,
                Recipes = theRecipes,
                Settings = new DeploymentSettings() { TargetDirectory = "target" }
            });

            plan.Settings.Environment.ShouldBeTheSameAs(theEnvironment);
            plan.Settings.Profile.ShouldBeTheSameAs(theProfile);
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



            plan.GetSubstitutionDiagnosticReport().ShouldHaveTheSameElementsAs(
                new SettingDataSource(){Key = "Env1", Provenance = "Environment settings", Value = "Env1-Val"},
                new SettingDataSource() { Key = "Profile1", Provenance = "Profile:  profile1", Value = "Profile1-Val" },
                new SettingDataSource() { Key = EnvironmentSettings.ROOT, Provenance = "DeploymentSettings", Value = "target" },
                new SettingDataSource() { Key = "Shared", Provenance = "Profile:  profile1", Value = "Shared-Profile-Val" }

                
                );

        }

        [Test]
        public void set_the_target_directory_if_root_exists_in_environment()
        {
            theEnvironment.Data[EnvironmentSettings.ROOT] = "env-root";

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
            theEnvironment.Data[EnvironmentSettings.ROOT] = "env-root";
            theProfile.Data[EnvironmentSettings.ROOT] = "profile-root";

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

            var sourcing = plan.GetSubstitutionDiagnosticReport().Single(x => x.Key == EnvironmentSettings.ROOT);
            sourcing.Value.ShouldEqual("settings-target");
            sourcing.Provenance.ShouldEqual("DeploymentSettings");
        
        }
    }
}