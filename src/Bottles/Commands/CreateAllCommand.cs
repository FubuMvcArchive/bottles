using System.ComponentModel;
using System.IO;
using System.Linq;
using Bottles.Creation;
using Bottles.Diagnostics;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Commands
{
    public class CreateAllInput
    {
        public CreateAllInput()
        {
            //TODO: make this work with binding
            TargetFlag = CompileTarget.Debug.ToString();
            DirectoryFlag = ".".ToFullPath();
        }

        [FlagAlias("output", 'o')]
        public string OutputFlag { get; set; }

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
        public string TargetFlag { get; set; }

        [Description("Directs the command to remove all bottle files before creating new files.  Can be destructive")]
        public bool CleanFlag { get; set; }

        

        public string DeploymentRoot()
        {
            string deploymentDirectory = DeploymentFlag ?? BottleFiles.BottlesFolder;
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
            var output = input.OutputFlag ?? "bottles";

            LogWriter.Current.Trace("Creating all packages from directory " + input.DirectoryFlag);

            LogWriter.Current.Indent(() =>
            {
                if (input.CleanFlag)
                {
                    LogWriter.Current.Trace("Removing all previous package files");
                    system.CleanDirectory(output);
                }

                LogWriter.Current.Trace("Looking for package manifest files starting at:");
                LogWriter.Current.Trace(input.DirectoryFlag); 
            });


            var results = PackageManifest.FindManifestFilesInDirectory(input.DirectoryFlag).Select(file =>
            {
                var folder = Path.GetDirectoryName(file);
                return createPackage(folder, output, input);
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