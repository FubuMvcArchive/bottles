using System.ComponentModel;
using FubuCore.CommandLine;
using FubuCore;

namespace Bottles.Commands
{
    public class OpenManifestInput
    {
        [Description("The physical path or alias for the Bottle directory")]
        public string BottleDirectory { get; set; }
    }

    [CommandDescription("Opens the package manifest file in the supplied directory in your text editor", Name = "open-manifest")]
    public class OpenManifestCommand : FubuCommand<OpenManifestInput>
    {
        public override bool Execute(OpenManifestInput input)
        {
            var directory = new AliasService().GetFolderForAlias(input.BottleDirectory);

            var packageFile = directory.AppendPath(PackageManifest.FILE);
            System.Console.WriteLine("Looking for " + packageFile);

            var system = new FileSystem();

            if (system.FileExists(packageFile))
            {
                System.Console.WriteLine("Opening file...");
                system.LaunchEditor(packageFile);
            }
            else
            {
                System.Console.WriteLine("Could not find a PackageManifest");
                System.Console.WriteLine("To create a new PackageManifest, use 'bottles init " + directory + " [PackageName]'");
            }




            return true;
        }
    }
}