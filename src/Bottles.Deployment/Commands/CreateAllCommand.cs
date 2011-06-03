using System;
using System.ComponentModel;
using System.IO;
using Bottles.Commands;
using Bottles.Creation;
using FubuCore;
using FubuCore.CommandLine;
using System.Collections.Generic;

namespace Bottles.Deployment.Commands
{
    public class CreateAllInput
    {
        public CreateAllInput()
        {
            TargetFlag = CompileTargetEnum.debug;
            DirectoryFlag = ".".ToFullPath();
        }

        [Description("Overrides the top level directory to begin searching for package manifests")]
        public string DirectoryFlag { get; set; }

        [Description("Overrides the deployment directory ~/deployment")]
        public string DeploymentFlag { get; set; }

        [Description("Includes any matching .pdb files for the package assemblies")]
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

            ConsoleWriter.Write("Creating all packages");

            if (input.CleanFlag)
            {
                ConsoleWriter.Write("  Removing all previous package files");
                system.CleanDirectory(settings.BottlesDirectory);
            }

            ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, 2,"Looking for package manifest files starting at:");
            ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, 2,input.DirectoryFlag);
            PackageManifest.FindManifestFilesInDirectory(input.DirectoryFlag).Each(file =>
            {
                var folder = Path.GetDirectoryName(file);
                createPackage(folder, settings.BottlesDirectory, input);
            });

            return true;
        }

        private static void createPackage(string packageFolder, string bottlesDirectory, CreateAllInput input)
        {
            if (packageFolder.IsEmpty()) return;

            var createInput = new CreatePackageInput(){
                PackageFolder = packageFolder,
                PdbFlag = input.PdbFlag,
                TargetFlag = input.TargetFlag,
                BottlesDirectory = bottlesDirectory
            };

            new CreatePackageCommand().Execute(createInput);
        }
    }
}