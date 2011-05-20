using FubuCore.Configuration;

namespace Bottles.Deployment.Configuration
{
    public class EnvironmentSettings : ProfileBase
    {
        public static readonly string EnvironmentSettingsFileName = "environment.settings";
        public static readonly string ROOT = "root";

        public EnvironmentSettings() : base(SettingCategory.environment, "Environment settings")
        {
        }


        public static EnvironmentSettings ReadFrom(string environmentFile)
        {
            var environment = new EnvironmentSettings();
            SettingsData.ReadFromFile(environmentFile, environment.Data);

            return environment;
        }
    }
}