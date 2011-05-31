using System.IO;
using Bottles.Deployment.Configuration;
using Bottles.Deployment.Runtime;
using System.Linq;
using FubuCore;

namespace Bottles.Deployment.Parsing
{
    public interface IDeploymentGraphReader
    {
        DeploymentGraph Read(DeploymentOptions options);
    }

    public class DeploymentGraphReader : IDeploymentGraphReader
    {
        private readonly DeploymentSettings _settings;

        public DeploymentGraphReader(DeploymentSettings settings)
        {
            _settings = settings;
        }

        public DeploymentGraph Read(DeploymentOptions options)
        {
            var allRecipes = _settings.Directories.Select(x => x.AppendPath(ProfileFiles.RecipesDirectory)).SelectMany(RecipeReader.ReadRecipes);
            return new DeploymentGraph{
                Environment = EnvironmentSettings.ReadFrom(_settings.EnvironmentFile()),
                Profile = Profile.ReadFrom(_settings, options.ProfileName),
                Recipes = allRecipes,
                Settings = _settings
            };
            
            /* DeploymentOptions.IncludedLinks
             * For env, look in primary first, then all secondaries
             * For Profile, look in primary first, then all secondaries
             * For Recipes, fully additive
             * 
             * Change BottleRepository.pathForBottle to look at multiple places
             * Need to add something to deployment options command line goop
             * 
             * 
             * FileSystem extension -> FindFileInDirectories(string fileName, IEnumerable<string> directories)
             */
        }
    }
}