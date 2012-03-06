using FubuCore.Configuration;

namespace Bottles.Deployment.Configuration
{
    public abstract class ProfileBase
    {
        private readonly SettingsData _data;

        protected ProfileBase(SettingCategory category, string provenance)
        {
            _data = new SettingsData(category){
                Provenance = provenance
            };
        }

        public SettingsData Data
        {
            get { return _data; }
        }

        public SettingsData DataForHost(string hostName)
        {
            var settings = _data.Child(hostName);
            settings.Category = _data.Category;

            return settings;
        }
    }
}