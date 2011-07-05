using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bottles.Deployers.Iis;
using Bottles.Deployment;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime;
using Bottles.Deployment.Writing;
using FubuCore;
using FubuCore.Configuration;
using StoryTeller;
using StoryTeller.Engine;
using StructureMap;

namespace Bottles.Storyteller.Fixtures
{
    public class DeploymentFixture : Fixture
    {
        public DeploymentFixture()
        {
            Title = "Bottles Deployment";

            this["Configure"] = Embed<DeploymentConfigurationFixture>("Given a deployment configuration that has");
            this["Options"] = Embed<DeploymentOptionsFixture>("When the deployment is executed with:");
            this["ReadingProfile"] = Embed<ProfileReaderFixture>("Then reading the deployment profile results in");
        }
    }


    public class DeploymentOptionsFixture : Fixture
    {
        private DeploymentOptions _options;

        public override void SetUp(ITestContext context)
        {
            _options = new DeploymentOptions();

            context.Store(_options);
        }

        [FormatAs("No profile is passed in explicitly")]
        public void NoProfileIsPassedInExplicitly()
        {
            //no-op
        }

        [FormatAs("Profile set to {profile}")]
        public void ProfileIs(string profile)
        {
            _options.ProfileName = profile;
        }

        [FormatAs("Recipes set to {recipes}")]
        public void Recipes(string[] recipes)
        {
            _options.RecipeNames.AddRange(recipes);
        }
    }

    public class DeploymentConfigurationFixture : Fixture
    {
        private DeploymentSettings _settings;
        private DeploymentWriter _writer;

        public override void SetUp(ITestContext context)
        {
            _settings = new DeploymentSettings("storyteller");
            context.Store(_settings);

            _writer = new DeploymentWriter("storyteller");
        }

        public override void TearDown()
        {
            Debug.WriteLine("Writing to " + _settings.DeploymentDirectory.ToFullPath());
            _writer.Flush(FlushOptions.Wipeout);
        }


        [ExposeAsTable("The environment settings are")]
        public void EnvironmentSettings(string Key, string Value)
        {
            _writer.AddEnvironmentSetting(Key, Value);
        }

        [ExposeAsTable("The profile settings are")]
        public void ProfileSettings(string ProfileName, string Key, string Value)
        {
            _writer.ProfileFor(ProfileName).AddProperty(Key, Value);
        }

        [FormatAs("A profile {profile} that depends on profile(s) {profileNames}")]
        public void ProfileProfiles(string profile, string[] profileNames)
        {
            var profileDef = _writer.ProfileFor(profile);
            profileNames.Each(profileDef.AddProfileDependency);
        }

        [FormatAs("A profile {profile} that contains the recipe(s) {recipeNames}")]
        public void ProfileRecipes(string profile, string[] recipeNames)
        {
            var profileDef = _writer.ProfileFor(profile);
            recipeNames.Each(profileDef.AddRecipe);
        }

        [FormatAs("The recipe {RecipeName} has host(s) {HostNames}")]
        public void X(string RecipeName, string[] HostNames)
        {
            HostNames.Each(hn => _writer.RecipeFor(RecipeName).HostFor(hn));
        }

        [FormatAs("A recipe {recipe} that is standalone")]
        public void RecipeStandalone(string recipe)
        {
            _writer.RecipeFor(recipe);
        }

        [FormatAs("A recipe {recipe} that depends on the recipe(s) {dependencies}")]
        public void RecipeDependencies(string recipe, string[] dependencies)
        {
            var recipeDef = _writer.RecipeFor(recipe);
            dependencies.Each(recipeDef.RegisterDependency);
        }

        [ExposeAsTable("The host directive values are")]
        public void HostValues(string Recipe, string Host, string Key, string Value)
        {
            _writer.RecipeFor(Recipe).HostFor(Host).AddProperty(Key, Value);
        }
    }


    public class ProfileReaderFixture : Fixture
    {
        private DeploymentOptions _deploymentOptions;
        private DeploymentSettings _deploymentSettings;
        private IEnumerable<SettingDataSource> _hostData;
        private DeploymentPlan _plan;

        public override void SetUp(ITestContext context)
        {
            _deploymentSettings = context.Retrieve<DeploymentSettings>();
            _deploymentOptions = context.Retrieve<DeploymentOptions>();

            var reader = new DeploymentGraphReader(_deploymentSettings);
            var graph = reader.Read(_deploymentOptions);
            _plan = new DeploymentPlan(_deploymentOptions, graph);
        }

        [FormatAs("All the properties for host {host} are")]
        public void FetchPropertiesForHost(string host)
        {
            var ahost = _plan.Hosts
                .Single(h => h.Name == host);

            _hostData = ahost.CreateDiagnosticReport();
        }

        

        public IGrammar CheckPropertiesForHost()
        {
            return VerifySetOf(() => _hostData)
                .Titled("The deployment properties are")
                .MatchOn(x => x.Key, x => x.Value, x => x.Provenance);
        }

        public IGrammar CheckHosts()
        {
            return VerifyStringList(findHosts)
                .Titled("The hosts in order are")
                .Ordered()
                .Grammar();
        }

        public IGrammar CheckRecipes()
        {
            return VerifyStringList(findRecipies)
                .Titled("The recipes in order are")
                .Ordered()
                .Grammar();
        }

        private IEnumerable<string> findRecipies()
        {
            return _plan.Recipes.Select(r => r.Name);
        }

        private IEnumerable<string> findHosts()
        {
            return _plan.Hosts.Select(h => h.Name);
        }

        [FormatAs("The property {propertyName} of the Website directive in host {host} is {value}")]
        public string HostWebsitePropertyIs(string host, string propertyName)
        {
            var registry = new DirectiveTypeRegistry(new Container());
            registry.AddType(typeof (Website));

            var factory = new DirectiveRunnerFactory(null, registry);
            var hostManifest = _plan.GetHost(host);
            factory.BuildDirectives(_plan, hostManifest, registry);
            var website = hostManifest.Directives.OfType<Website>().Single();

            var property = typeof (Website).GetProperty(propertyName);
            return property.GetValue(website, null) as string;
        }

        [FormatAs("The environment setting {setting} is {value}")]
        public string CheckEnvSetting(string setting)
        {
            //any way to get the SettingsProvider?

            return _deploymentSettings.Environment.GetKey(setting).ToString();
        }
    }
}