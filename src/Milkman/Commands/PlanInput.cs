using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Bottles.Deployment.Runtime;
using FubuCore;
using FubuCore.CommandLine;
using System.Linq;

namespace Bottles.Deployment.Commands
{
    public class PlanInput
    {
        [Description("The profile to execute.  'default' is the default.")]
        public string ProfileFlag { get; set; }

        [Description("Path to where the deployment folder is ~/deployment")]
        public string DeploymentFlag { get; set; }

        [Description("Import any other ~/deployment folders for this deployment")]
        [RequiredUsage("imports")]
        // TODO -- want an end to end test on this
        public IList<string> ImportedFolders { get; set; }

        [Description("Tacks on ONE additional recipie. Great for including tests.")] //until fubu command gets better at parsing command lines
        public string RecipeFlag { get; set; }

        [Description("File where the installation report should be written.  Default is installation_report.htm")]
        public string ReportFlag { get; set; }

        protected virtual void enhanceDeploymentOptions(DeploymentOptions options)
        {

        }

        public DeploymentOptions CreateDeploymentOptions()
        {
            var options = new DeploymentOptions(GetProfile()){
                ReportName = ReportFlag
            };
            enhanceDeploymentOptions(options);

            if(RecipeFlag != null)
            {
                options.RecipeNames.Fill(RecipeFlag);
            }

            if (ImportedFolders != null)
            {
                options.ImportedFolders.AddRange(ImportedFolders);
            }

            return options;
        }

        public string GetProfile()
        {
            if(ProfileFlag.IsNotEmpty())
            {
                return ProfileFlag;
            }

            var profile = "default";
            if(ProfileFlag.IsEmpty())
            {
                var dir = ".".ToFullPath().AppendPath(GetDeployment(), ProfileFiles.ProfilesDirectory);

                if(!Directory.Exists(dir))
                {
                    return profile;
                }

                var files = Directory.GetFiles(dir);
                if(files.Count()==1)
                {
                    profile = files.First();
                    profile = Path.GetFileNameWithoutExtension(profile);
                }
            }

            return profile;
        }

        public string GetDeployment()
        {
            return DeploymentFlag ?? ".".ToFullPath().AppendPath(ProfileFiles.DeploymentFolder);
        }
    }
}