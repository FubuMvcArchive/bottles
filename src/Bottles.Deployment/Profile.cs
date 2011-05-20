using System;
using System.Collections.Generic;
using Bottles.Deployment.Configuration;
using FubuCore;
using FubuCore.Configuration;

namespace Bottles.Deployment
{
    public class Profile : ProfileBase
    {
        public static readonly string RecipePrefix = "recipe:";
        private readonly IList<string> _recipes = new List<string>();

        public Profile(string profileName) : base(SettingCategory.profile, "Profile:  " + profileName)
        {
        }

        public IEnumerable<string> Recipes
        {
            get { return _recipes; }
        }

        public static Profile ReadFrom(DeploymentSettings settings, string profileName)
        {
            var profile = new Profile(profileName);
            var profileFile = settings.GetProfile(profileName);

            var fileSystem = new FileSystem();
            if (!fileSystem.FileExists(profileFile))
            {
                throw new Exception("Couldn't find the profile '{0}'".ToFormat(profileFile));
            }

            fileSystem.ReadTextFile(profileFile, profile.ReadText);

            return profile;
        }

        public void ReadText(string text)
        {
            if (text.IsEmpty()) return;

            if (text.StartsWith(RecipePrefix))
            {
                var recipeName = text.Substring(RecipePrefix.Length).Trim();
                AddRecipe(recipeName);
            }
            else
            {
                Data.Read(text);
            }
        }

        public void AddRecipe(string recipe)
        {
            _recipes.Fill(recipe);
        }
    }
}