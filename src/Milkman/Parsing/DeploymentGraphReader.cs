using Bottles.Deployment.Configuration;
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
            _settings.AddImportedFolders(options.ImportedFolders);

            var allRecipes = _settings.AllRecipies();
            var env = EnvironmentSettings.ReadFrom(_settings.EnvironmentFile);
            var profile = Profile.ReadFrom(_settings, options.ProfileName);

            return new DeploymentGraph {
                Environment = env,
                Profile = profile,
                Recipes = allRecipes,
                Settings = _settings
            };
        }
    }
}