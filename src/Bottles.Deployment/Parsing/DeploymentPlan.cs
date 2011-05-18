using System;
using System.Collections.Generic;
using Bottles.Configuration;
using Bottles.Deployment.Runtime;

namespace Bottles.Deployment.Parsing
{
    public class DeploymentPlan
    {
        private readonly DeploymentOptions _options;

        public DeploymentPlan(DeploymentOptions options)
        {
            _options = options;
        }

        public DeploymentOptions Options
        {
            get { return _options; }
        }

        public IEnumerable<Recipe> Recipes { get; private set; }
        public IEnumerable<HostManifest> Hosts { get; set; }
        public Profile Profile { get; private set; }
        public EnvironmentSettings Environment { get; private set; }

        public void AddRecipes(IEnumerable<Recipe> recipes)
        {
            Recipes = recipes;
        }

        public void AddHosts(IEnumerable<HostManifest> hosts)
        {
            Hosts = hosts;
        }


        private readonly IList<OverrideSource> _overrideSourcing = new List<OverrideSource>();

        public void ReadProfileAndSettings(EnvironmentSettings environment, Profile profile)
        {
            Environment = environment;
            Profile = profile;

            Profile.Overrides.Each((k, v) =>
            {
                Environment.Overrides[k] = v;
            });

            Environment.Overrides.Each((key, value) => _overrideSourcing.Add(new OverrideSource()
            {
                Key = key,
                Value = value,
                Provenance = Profile.Overrides.Has(key) ? "Profile" : "Environment"
            }));
        }

        // TODO -- unit test this
        public IEnumerable<OverrideSource> OverrideSourcing
        {
            get { return _overrideSourcing; }
        }

        public IDictionary<string, string> Substitutions
        {
            get { return Environment.Overrides.ToDictionary(); }
        }
    }



    public class OverrideSource
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Provenance { get; set; }

        public bool Equals(OverrideSource other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Key, Key) && Equals(other.Value, Value) && Equals(other.Provenance, Provenance);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (OverrideSource)) return false;
            return Equals((OverrideSource) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Key != null ? Key.GetHashCode() : 0);
                result = (result*397) ^ (Value != null ? Value.GetHashCode() : 0);
                result = (result*397) ^ (Provenance != null ? Provenance.GetHashCode() : 0);
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format("Key: {0}, Value: {1}, Provenance: {2}", Key, Value, Provenance);
        }
    }
}