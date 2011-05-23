using Bottles.Deployment.Configuration;
using FubuCore;

namespace Bottles.Deployment.Deployers.Configuration
{
    public class CentralConfig : IDirective
    {
        public CentralConfig()
        {
            Directory = FileSystem.Combine(EnvironmentSettings.ROOT.ToSubstitution(), "config");
        }

        public string Directory { get; set; }
    }
}