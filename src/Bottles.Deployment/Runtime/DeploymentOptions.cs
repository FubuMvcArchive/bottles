using System.Collections.Generic;

namespace Bottles.Deployment.Runtime
{
    public class DeploymentOptions
    {
        private readonly IList<string> _recipeNames = new List<string>();

        public DeploymentOptions() : this("default")
        {
        }

        public DeploymentOptions(string profileName)
        {
            ProfileName = profileName;
            ReportName = "report.htm";
        }

        public string ProfileName { get; set; }
        public string ReportName { get; set; }

        public IList<string> RecipeNames
        {
            get { return _recipeNames; }
        }
    }
}