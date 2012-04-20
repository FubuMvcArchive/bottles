using System.ComponentModel;
using System.IO;
using System.Linq;
using Bottles.Commands;
using Bottles.Creation;
using Bottles.Diagnostics;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class CreateAllInput
    {
        public CreateAllInput()
        {
            TargetFlag = CompileTargetEnum.Debug;
            DirectoryFlag = ".".ToFullPath();
        }

        [Description("Overrides the top level directory to begin searching for package manifests")]
        [FlagAlias("directory", 'd')]
        public string DirectoryFlag { get; set; }

        [Description("Overrides the deployment directory ~/deployment")]
        [FlagAlias("deployment", 'y')]
        public string DeploymentFlag { get; set; }

        [Description("Includes any matching .pdb files for the package assemblies")]
        [FlagAlias("pdb", 'b')]
        public bool PdbFlag { get; set; }

        [Description("Overrides the compilation target.  The default is debug")]
        public CompileTargetEnum TargetFlag { get; set; }

        [Description("Directs the command to remove all bottle files before creating new files.  Can be destructive")]
        public bool CleanFlag { get; set; }

        public string DeploymentRoot()
        {
            string deploymentDirectory = DeploymentFlag ?? ProfileFiles.DeploymentFolder;
            return deploymentDirectory;
        }
    }

    [CommandDescription("Creates all the packages for the directories / manifests listed in the bottles.manifest file and puts the new packages into the deployment/bottles directory", Name="create-all")]
    public class CreateAllCommand : FubuCommand<CreateAllInput>
    {
        public override bool Execute(CreateAllInput input)
        {
            return Execute(new FileSystem(), input);
        }

        public bool Execute(IFileSystem system, CreateAllInput input)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);

            LogWriter.Current.Trace("Creating all packages from directory " + input.DirectoryFlag);

            LogWriter.Current.Indent(() =>
            {
                if (input.CleanFlag)
                {
                    LogWriter.Current.Trace("Removing all previous package files");
                    system.CleanDirectory(settings.BottlesDirectory);
                }

                LogWriter.Current.Trace("Looking for package manifest files starting at:");
                LogWriter.Current.Trace(input.DirectoryFlag); 
            });


            var results = PackageManifest.FindManifestFilesInDirectory(input.DirectoryFlag).Select(file =>
            {
                var folder = Path.GetDirectoryName(file);
                return createPackage(folder, settings.BottlesDirectory, input);
            });

            return results.Any(r => !r);
        }

        private static bool createPackage(string packageFolder, string bottlesDirectory, CreateAllInput input)
        {
            if (packageFolder.IsEmpty()) return true;

            var createInput = new CreateBottleInput(){
                PackageFolder = packageFolder,
                PdbFlag = input.PdbFlag,
                TargetFlag = input.TargetFlag,
                BottlesDirectory = bottlesDirectory
            };

            return new CreateBottleCommand().Execute(createInput);
        }
    }
}