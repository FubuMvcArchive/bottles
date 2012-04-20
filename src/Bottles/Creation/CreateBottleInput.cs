using System.ComponentModel;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Creation
{
    public enum CompileTargetEnum
    {
        Debug,
        Release
    }

    public class CreateBottleInput
    {
        public CreateBottleInput()
        {
            TargetFlag = CompileTargetEnum.Debug;
        }

        [Description("The root physical folder (or valid alias) of the package")]
        [FlagAlias("packagefolder", 'p')]
        public string PackageFolder { get; set; }

        [Description("The filepath where the zip file for the package will be written ie. ./deployment/bottles/my-btl.zip")]
        [FlagAlias("output", 'o')]
        public string ZipFileFlag { get; set; }

        [IgnoreOnCommandLine]
        public string BottlesDirectory { get; set;}

        [Description("Includes any matching .pdb files for the package assemblies")]
        [FlagAlias("pdb", 'b')]
        public bool PdbFlag { get; set; }

        [Description("Forces the command to delete any existing zip file first")]
        [FlagAlias("force", 'f')]
        public bool ForceFlag { get; set; }

        [Description("Choose the compilation target for any assemblies")]
        public CompileTargetEnum TargetFlag { get; set; }

        [Description("Overrides the name of the manifest file (defaults to '" + PackageManifest.FILE + "'")]
        [FlagAlias("manifest", 'm')]
        public string ManifestFileNameFlag { get; set; }

        public string GetZipFileName(PackageManifest manifest)
        {
            return ZipFileFlag ?? FileSystem.Combine(BottlesDirectory, manifest.Name + ".zip");
        }
    }
}