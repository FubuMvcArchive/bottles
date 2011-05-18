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
        [Test]
        public void combine_overrides_and_provenance()
        {
            var environment = new EnvironmentSettings();
            environment.Overrides["Env1"] = "Env1-Val";
            environment.Overrides["Shared"] = "Shared-Env-Val";

            var profile = new Profile();
            profile.Overrides["Shared"] = "Shared-Profile-Val";
            profile.Overrides["Profile1"] = "Profile1-Val";

            var plan = new DeploymentPlan(new DeploymentOptions());
            plan.ReadProfileAndSettings(environment, profile);

            plan.OverrideSourcing.ShouldHaveTheSameElementsAs(
                new OverrideSource(){Key = "Env1", Provenance = "Environment", Value = "Env1-Val"},
                new OverrideSource(){Key = "Shared", Provenance = "Profile", Value = "Shared-Profile-Val"},
                new OverrideSource(){Key = "Profile1", Provenance = "Profile", Value = "Profile1-Val"}
                );

        }
    }
}