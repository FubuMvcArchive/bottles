using System;
using System.Collections.Generic;
using System.Linq;
using Bottles.Configuration;
using Bottles.Deployment.Runtime;
using FubuCore;

namespace Bottles.Deployment.Parsing
{

    public class DeploymentGraph
    {
        public EnvironmentSettings Environment { get; set; }
        public IEnumerable<Recipe> Recipes { get; set; }
        public Profile Profile { get; set; }
    }

    public class DeploymentGraphReader
    {
        private readonly DeploymentSettings _settings;
        private readonly IFileSystem _fileSystem;

        public DeploymentGraphReader(DeploymentSettings settings, IFileSystem fileSystem)
        {
            _settings = settings;
            _fileSystem = fileSystem;
        }

        public DeploymentGraph Read(DeploymentOptions options)
        {
            return new DeploymentGraph(){
                Environment = EnvironmentSettings.ReadFrom(_settings.EnvironmentFile),
                Profile = Profile.ReadFrom(_settings.GetProfile(options.ProfileName)),
                Recipes = RecipeReader.ReadRecipes(_settings.RecipesDirectory)
            };
        }
    }

    public class ProfileReader : IProfileReader
    {
        private readonly IRecipeSorter _sorter;
        private readonly DeploymentSettings _settings;
        private readonly IFileSystem _fileSystem;

        public ProfileReader(IRecipeSorter sorter, DeploymentSettings settings, IFileSystem fileSystem)
        {
            _sorter = sorter;
            _fileSystem = fileSystem;
            _settings = settings;
        }

        

        public DeploymentPlan Read(DeploymentOptions options)
        {
            var environment = EnvironmentSettings.ReadFrom(_settings.EnvironmentFile);

            return Read(options, environment);
        }

        public DeploymentPlan Read(DeploymentOptions options, EnvironmentSettings environment)
        {
            var deploymentPlan = new DeploymentPlan(options);

            environment.SetRootSetting(_settings.TargetDirectory);



            var profile = readProfile(options);

            var recipes = readRecipes(environment, options, profile);

            deploymentPlan.AddRecipes(recipes);

            var hosts = collateHosts(recipes);

            deploymentPlan.ReadProfileAndSettings(environment, profile);

            addProfileSettingsToHosts(profile, hosts);

            addEnvironmentSettingsToHosts(environment, hosts);
            
            deploymentPlan.AddHosts(hosts);
            
            return deploymentPlan;
        }

        private Profile readProfile(DeploymentOptions options)
        {
            var profile = new Profile();
            var profileFile = _settings.GetProfile(options.ProfileName);
            
            if(!_fileSystem.FileExists(profileFile))
            {
                throw new Exception("Couldn't find the profile '{0}'".ToFormat(profileFile));
            }

            _fileSystem.ReadTextFile(profileFile, profile.ReadText);

            return profile;
        }

        private IEnumerable<HostManifest> collateHosts(IEnumerable<Recipe> recipes)
        {
            if (recipes == null || !recipes.Any())
            {
                throw new Exception("Bah! no recipies");
            }
            

            var firstRecipe = recipes.First();
            recipes.Skip(1).Each(firstRecipe.AppendBehind);

            return firstRecipe.Hosts;
        }

        private IEnumerable<Recipe> readRecipes(EnvironmentSettings environment, DeploymentOptions options, Profile profile)
        {
            var recipes = RecipeReader.ReadRecipes(_settings.RecipesDirectory);
            recipes = buildEntireRecipeGraph(profile, options, recipes);
            // TODO -- log which recipes were selected
            recipes = _sorter.Order(recipes);
            return recipes;
        }

        public static IEnumerable<Recipe> buildEntireRecipeGraph(Profile profile, DeploymentOptions options, IEnumerable<Recipe> allRecipesAvailable)
        {
            var recipesToRun = new List<string>();

            recipesToRun.AddRange(profile.Recipes);
            recipesToRun.AddRange(options.RecipeNames);

            var dependencies = new List<string>();

            recipesToRun.Each(r =>
            {
                var rec = allRecipesAvailable.Single(x => x.Name == r);
                dependencies.AddRange(rec.Dependencies);
            });

            recipesToRun.AddRange(dependencies.Distinct());

            return recipesToRun.Distinct().Select(name => allRecipesAvailable.Single(o => o.Name == name));
        }

        private static void addProfileSettingsToHosts(Profile profile, IEnumerable<HostManifest> hosts)
        {
            hosts.Each(host => host.RegisterSettings(profile.DataForHost(host.Name)));
        }

        private static void addEnvironmentSettingsToHosts(EnvironmentSettings environment, IEnumerable<HostManifest> hosts)
        {
            hosts.Each(host => host.RegisterSettings(environment.DataForHost(host.Name)));
        }
    }
}