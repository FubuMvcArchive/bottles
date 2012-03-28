using Bottles.Creation;
using Bottles.Diagnostics;
using Bottles.PackageLoaders.Assemblies;
using Bottles.Zipping;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Commands
{
    [CommandDescription("Create a bottle zip from a package directory", Name = "create")]
    public class CreateBottleCommand : FubuCommand<CreateBottleInput>
    {
        public override bool Execute(CreateBottleInput input)
        {
            ConsoleWriter.Write("  Creating bottle at " + input.PackageFolder);

            input.PackageFolder = AliasCommand.AliasFolder(input.PackageFolder);

            Execute(input, new FileSystem());
            return true;
        }

        public void Execute(CreateBottleInput input, IFileSystem fileSystem)
        {
            //TODO: harden
            if (fileSystem.FileExists(input.ZipFileFlag) && !input.ForceFlag)
            {
                WriteZipFileAlreadyExists(input.ZipFileFlag);
                return;
            }

            // Delete the file if it exists?
            if (fileSystem.PackageManifestExists(input.PackageFolder))
            {
                fileSystem.DeleteFile(input.ZipFileFlag);
                CreatePackage(input, fileSystem);
            }
            else
            {
                WritePackageManifestDoesNotExist(input.PackageFolder);
            }
        }

        public virtual void WriteZipFileAlreadyExists(string zipFileName)
        {
            ConsoleWriter.Write("Package Zip file already exists at '{0}'.  Use the -f (force) flag to overwrite the existing flag", zipFileName);
        }

        public virtual void WritePackageManifestDoesNotExist(string packageFolder)
        {
            ConsoleWriter.Write(
                "The requested package folder at '{0}' does not have a package manifest.  Run 'blue init-pak \"{0}\"' first.",
                packageFolder);
        }

        public virtual void CreatePackage(CreateBottleInput input, IFileSystem fileSystem)
        {
            var fileName = FileSystem.Combine(input.PackageFolder, input.ManifestFileNameFlag ?? PackageManifest.FILE);
            var manifest = fileSystem.LoadFromFile<PackageManifest>(fileName);
            
            var creator = new PackageCreator(fileSystem, new ZipFileService(fileSystem), new PackageLogger(), new AssemblyFileFinder(fileSystem));
            creator.CreatePackage(input, manifest);
        }
    }
}