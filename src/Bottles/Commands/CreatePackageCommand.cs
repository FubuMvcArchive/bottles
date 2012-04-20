using System;
using System.ComponentModel;
using Bottles.Creation;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Commands
{
    public class CreatePackageInput
    {
        public CreatePackageInput()
        {
            TargetFlag = CompileTargetEnum.Debug;
        }

        [Description("The root physical folder (or valid alias) of the package")]
        [FlagAlias("packagefolder", 'p')]
        public string PackageFolder { get; set; }

        [Description("The filepath where the zip file for the package will be written ie. ./blue/my-pak.zip")]
        public string ZipFile { get; set; }

        [IgnoreOnCommandLine]
        public string BottlesDirectory { get; set; }

        [Description("Includes any matching .pdb files for the package assemblies")]
        [FlagAlias("pdb", 'b')]
        public bool PdbFlag { get; set; }

        [Description("Forces the command to delete any existing zip file first")]
        [FlagAlias("force", 'f')]
        public bool ForceFlag { get; set; }

        [Description("Choose the compilation target for any assemblies")]
        public CompileTargetEnum TargetFlag { get; set; }

        [Description("Overrides the name of the manifest file")]
        [FlagAlias("file", 'm')]
        public string ManifestFileNameFlag { get; set; }

        public string GetZipFileName(PackageManifest manifest)
        {
            return ZipFile ?? FileSystem.Combine(BottlesDirectory, manifest.Name + ".zip");
        }
    }

    [CommandDescription("Create a package file from a package directory", Name = "create-pak")]
    public class CreatePackageCommand : FubuCommand<CreatePackageInput>
    {
        public override bool Execute(CreatePackageInput input)
        {
            ConsoleWriter.Write(ConsoleColor.Red,"This method is obsolete, use 'create' instead");
            var input2 = new CreateBottleInput()
                         {
                             PackageFolder = input.PackageFolder,
                             ZipFileFlag = input.ZipFile,
                             BottlesDirectory = input.BottlesDirectory,
                             PdbFlag = input.PdbFlag,
                             ForceFlag = input.ForceFlag,
                             TargetFlag = input.TargetFlag,
                             ManifestFileNameFlag = input.ManifestFileNameFlag
                         };
            return new CreateBottleCommand().Execute(input2);
        }
    }
}