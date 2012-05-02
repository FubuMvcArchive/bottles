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

            input.PackageFolder = new AliasService().GetFolderForAlias(input.PackageFolder);

            return Execute(input, new FileSystem());
        }

        public bool Execute(CreateBottleInput input, IFileSystem fileSystem)
        {
            //TODO: harden
            if (fileSystem.FileExists(input.ZipFileFlag) && !input.ForceFlag)
            {
                WriteZipFileAlreadyExists(input.ZipFileFlag);
                return true;
            }

            // Delete the file if it exists?
            if (fileSystem.PackageManifestExists(input.PackageFolder))
            {
                fileSystem.DeleteFile(input.ZipFileFlag);
                return CreatePackage(input, fileSystem);
            }

            WritePackageManifestDoesNotExist(input.PackageFolder);
            return false;
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

        public virtual bool CreatePackage(CreateBottleInput input, IFileSystem fileSystem)
        {
            var fileName = FileSystem.Combine(input.PackageFolder, input.ManifestFileNameFlag ?? PackageManifest.FILE);
            var manifest = fileSystem.LoadFromFile<PackageManifest>(fileName);
            
            var creator = new PackageCreator(fileSystem, new ZipFileService(fileSystem), new PackageLogger(new LoggingSession()), new AssemblyFileFinder(fileSystem));
            return creator.CreatePackage(input, manifest);
        }
    }
}