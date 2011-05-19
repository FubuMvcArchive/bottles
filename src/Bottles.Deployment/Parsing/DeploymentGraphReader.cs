using Bottles.Configuration;
using Bottles.Deployment.Runtime;

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
            return new DeploymentGraph{
                Environment = EnvironmentSettings.ReadFrom(_settings.EnvironmentFile),
                Profile = Profile.ReadFrom(_settings.GetProfile(options.ProfileName)),
                Recipes = RecipeReader.ReadRecipes(_settings.RecipesDirectory),
                Settings = _settings
            };
        }
    }
}