using Bottles.Configuration;
using Bottles.Deployment.Runtime;

namespace Bottles.Deployment.Parsing
{
    public class DeploymentGraphReader
    {
        private readonly DeploymentSettings _settings;

        public DeploymentGraphReader(DeploymentSettings settings)
        {
            _settings = settings;
        }

        public DeploymentGraph Read(DeploymentOptions options)
        {
            return new DeploymentGraph{
                Environment = EnvironmentSettings.ReadFrom(_settings.EnvironmentFile),
                Profile = Profile.ReadFrom(_settings.GetProfile(options.ProfileName)),
                Recipes = RecipeReader.ReadRecipes(_settings.RecipesDirectory),
                Settings = _settings
            };
        }
    }
}