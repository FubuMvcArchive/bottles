using System;
using System.Collections.Generic;
using System.Linq;
using Bottles.Configuration;
using Bottles.Deployment.Runtime;

namespace Bottles.Deployment.Parsing
{
    // TODO --
    /*
     *  Set the root on the settings
     *  
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     */

    public class DeploymentPlan
    {
        private readonly DeploymentGraph _graph;
        private readonly IEnumerable<HostManifest> _hosts;
        private readonly DeploymentOptions _options;
        private readonly IList<OverrideSource> _overrideSourcing = new List<OverrideSource>();
        private readonly IEnumerable<Recipe> _recipes;

        public DeploymentPlan(DeploymentOptions options, DeploymentGraph graph)
        {
            _options = options;
            _graph = graph;
            _recipes = readRecipes();

            readProfileAndSettings();

            _hosts = collateHosts(_recipes);

            addProfileSettings();
            addEnvironmentSettings();

            readRoot();
        }

        private void readRoot()
        {
            if (Substitutions.ContainsKey(EnvironmentSettings.ROOT))
            {
                _graph.Settings.TargetDirectory = Substitutions[EnvironmentSettings.ROOT];
            }
            else
            {
                _graph.Environment.Overrides[EnvironmentSettings.ROOT] = _graph.Settings.TargetDirectory;
                _overrideSourcing.Add(new OverrideSource(){
                    Key = EnvironmentSettings.ROOT,
                    Provenance = typeof(DeploymentSettings).Name,
                    Value = _graph.Settings.TargetDirectory
                });
            }
        }

        private void addEnvironmentSettings()
        {
            _hosts.Each(host => host.RegisterSettings(_graph.Environment.DataForHost(host.Name)));
        }

        private void addProfileSettings()
        {
            _hosts.Each(host => host.RegisterSettings(_graph.Profile.DataForHost(host.Name)));
        }

        public DeploymentOptions Options
        {
            get { return _options; }
        }

        public IEnumerable<Recipe> Recipes
        {
            get { return _recipes; }
        }


        public IEnumerable<HostManifest> Hosts
        {
            get { return _hosts; }
        }

        public IEnumerable<OverrideSource> OverrideSourcing
        {
            get { return _overrideSourcing; }
        }

        public IDictionary<string, string> Substitutions
        {
            get { return _graph.Environment.Overrides.ToDictionary(); }
        }

        private static IEnumerable<HostManifest> collateHosts(IEnumerable<Recipe> recipes)
        {
            if (recipes == null)
            {
                throw new Exception("Bah! no recipies");
            }


            var firstRecipe = recipes.First();
            recipes.Skip(1).Each(firstRecipe.AppendBehind);

            return firstRecipe.Hosts;
        }

        private IEnumerable<Recipe> readRecipes()
        {;
            var recipes = buildEntireRecipeGraph(_graph.Recipes);

            // TODO -- log which recipes were selected
            recipes = new RecipeSorter().Order(recipes);
            return recipes;
        }

        private IEnumerable<Recipe> buildEntireRecipeGraph(IEnumerable<Recipe> allRecipesAvailable)
        {
            var recipesToRun = new List<string>();

            recipesToRun.AddRange(_graph.Profile.Recipes);
            recipesToRun.AddRange(_options.RecipeNames);

            var dependencies = new List<string>();

            recipesToRun.Each(r =>
            {
                var rec = allRecipesAvailable.Single(x => x.Name == r);
                dependencies.AddRange(rec.Dependencies);
            });

            recipesToRun.AddRange(dependencies.Distinct());

            return recipesToRun.Distinct().Select(name => allRecipesAvailable.Single(o => o.Name == name));
        }

        private void readProfileAndSettings()
        {
            _graph.Profile.Overrides.Each((k, v) => { _graph.Environment.Overrides[k] = v; });

            _graph.Environment.Overrides.Each((key, value) => _overrideSourcing.Add(new OverrideSource{
                Key = key,
                Value = value,
                Provenance = _graph.Profile.Overrides.Has(key) ? "Profile" : "Environment"
            }));
        }

        public HostManifest GetHost(string hostName)
        {
            return Hosts.FirstOrDefault(x => x.Name == hostName);
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
                var result = (Key != null ? Key.GetHashCode() : 0);
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