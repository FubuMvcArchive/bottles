using System.Collections.Generic;
using System.Diagnostics;
using FubuCore.Util;

namespace Bottles.Deployment.Runtime
{
    [DebuggerDisplay("Options for {ProfileName}")]
    public class DeploymentOptions
    {
        public DeploymentOptions() : this("default")
        {
        }

        public DeploymentOptions(string profileName)
        {
            ProfileName = profileName;
            ReportName = "report.htm";
            RecipeNames = new List<string>();
            ImportedFolders = new List<string>();
            Overrides = new Cache<string, string>();
        }

        public string ProfileName { get; set; }
        public string ReportName { get; set; }
        public IList<string> RecipeNames { get; private set; }
        public IList<string> ImportedFolders { get; private set; }
        public Cache<string, string> Overrides { get; private set; }       
    }
}