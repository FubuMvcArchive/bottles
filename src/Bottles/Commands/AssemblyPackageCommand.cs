using System;
using System.ComponentModel;
using System.Xml;
using Bottles.Zipping;
using FubuCore;
using FubuCore.CommandLine;
using System.Linq;
using System.Collections.Generic;
using FubuCsProjFile;

namespace Bottles.Commands
{
    public class AssemblyPackageInput
    {
        [Description("The root folder for the project if different from the project file's folder")]
        public string RootFolder { get; set; }

        [Description("Explicitly define the csproj file in this directory.  If not set, this command will look for a single csproj file in the directory to attach the embedded resources")]
        public string ProjFileFlag { get; set; }

        [Description("Previews which files will be added to the assembly bottle")]
        public bool PreviewFlag { get; set; }

        [Description("If selected, will intitialize a default PackageManifest file and embed it into the assembly for more fine-grained control over the Bottle properties")]
        public bool InitFlag { get; set; }

        public string FindCsProjFile()
        {
            return RootFolder.AppendPath(ProjFileFlag);
        }
    }

    [CommandDescription("Bundle up the content and data files for a self contained assembly package", Name = "assembly-pak")]
    public class AssemblyPackageCommand : FubuCommand<AssemblyPackageInput>
    {
        readonly IFileSystem fileSystem = new FileSystem();

        public override bool Execute(AssemblyPackageInput input)
        {
            input.RootFolder = new AliasService().GetFolderForAlias(input.RootFolder);

            determineMissingCsProjFile(input);

            if (input.PreviewFlag)
            {
                return displayPreview(input);
            }

            if (input.InitFlag)
            {
                createNewManifest(input);
                return true;
            }

            var zipService = new ZipFileService(fileSystem);

            var manifest = fileSystem.LoadPackageManifestFrom(input.RootFolder);
            if (manifest == null)
            {
                System.Console.WriteLine("No PackageManifest found, using defaults instead");
                manifest = new PackageManifest();
                manifest.SetRole(BottleRoles.Module);


                System.Console.WriteLine("WebContent:  " + manifest.ContentFileSet);
            }

            createZipFile(input, BottleFiles.WebContentFolder, zipService, manifest.ContentFileSet);
            createZipFile(input, BottleFiles.DataFolder, zipService, BottleFiles.DataFiles);
            createZipFile(input, BottleFiles.ConfigFolder, zipService, BottleFiles.ConfigFiles);


            return true;
        }

        private void createNewManifest(AssemblyPackageInput input)
        {
            
            var filename = input.RootFolder.AppendPath(PackageManifest.FILE);

            if (fileSystem.FileExists(filename))
            {
                Console.WriteLine("File already exists at " + filename);
            }
            else
            {
                Console.WriteLine("Writing new package manifest to " + filename);
                var manifest = PackageManifest.DefaultModuleManifest();
                fileSystem.WriteObjectToFile(filename, manifest);
            }

            Console.WriteLine("Adding an embedded resource for '{0}' to {1}", PackageManifest.FILE, input.ProjFileFlag);
            attachZipFileToProjectFile(input, PackageManifest.FILE);


            Console.WriteLine("Use 'bottles open-manifest {0}' to open and edit the PackageManifest", input.RootFolder);

            
        }

        private bool displayPreview(AssemblyPackageInput input)
        {
            var manifest = fileSystem.LoadPackageManifestFrom(input.RootFolder);
            if (manifest == null)
            {
                System.Console.WriteLine("Package manifest file {0} was not found", input.RootFolder.AppendPath(PackageManifest.FILE));
                return false;
            }

            previewFiles(input, BottleFiles.WebContentFolder, manifest.ContentFileSet);
            previewFiles(input, BottleFiles.DataFolder, BottleFiles.DataFiles);
            previewFiles(input, BottleFiles.ConfigFolder, BottleFiles.ConfigFiles);
            return true;
        }

        private void determineMissingCsProjFile(AssemblyPackageInput input)
        {
            if (input.ProjFileFlag.IsEmpty())
            {
                var files = fileSystem.FindFiles(input.RootFolder, new FileSet {Include = "*.csproj"});
                if (files.Count() == 1)
                {
                    System.Console.WriteLine("Found 1 csproj file");
                    System.Console.WriteLine("Using " + files.Single());
                    input.ProjFileFlag = files.Single().ToFullPath();
                }

                if (files.Count() > 1)
                {
                    System.Console.WriteLine(
                        "Found more than one *.csproj file in this directory.  You'll need to specify the --proj-file flag");
                }
            }
        }

        private void previewFiles(AssemblyPackageInput input, string folder, FileSet fileSearch)
        {
            if (fileSearch == null)
            {
                System.Console.WriteLine("{0}:  No files", folder);
                return;
            }

            var fileSystem = new FileSystem();
            var contentFolder = input.RootFolder.AppendPath(folder);

            var files = fileSystem.DirectoryExists(contentFolder) ? fileSystem.FindFiles(contentFolder, fileSearch) : fileSystem.FindFiles(input.RootFolder, fileSearch);

            if (files.Any())
            {
                System.Console.WriteLine("{0}: {1} file(s)", folder, files.Count());
                files.Each(f => System.Console.WriteLine(f));
            }
            else
            {
                System.Console.WriteLine("{0}:  No files", folder);
            }

            System.Console.WriteLine();
        }

        private void createZipFile(AssemblyPackageInput input, string childFolderName, ZipFileService zipService, FileSet fileSearch)
        {
            var zipRequest = BuildZipRequest(input, fileSearch);
            if (zipRequest == null)
            {
                ConsoleWriter.Write("No content for " + childFolderName);

                return;
            }

            // Hackey, but it makes /data and data/*.* work
            if (fileSystem.DirectoryExists(input.RootFolder.AppendPath(childFolderName)) && zipRequest.FileSet.Include.StartsWith(childFolderName, StringComparison.InvariantCultureIgnoreCase))
            {
                zipRequest.FileSet = FileSet.Deep("*.*");
                zipRequest.RootDirectory = zipRequest.RootDirectory.AppendPath(childFolderName);
            }

            var zipFileName = "pak-{0}.zip".ToFormat(childFolderName);

            // do not create a zip file if there's no files there.
            if (!fileSystem.FindFiles(input.RootFolder, zipRequest.FileSet).Any())
            {
                Console.WriteLine("No matching files for Bottle directory " + childFolderName);

                var projectFileName = input.FindCsProjFile();
                var csProjFile = CsProjFile.LoadFrom(projectFileName);
                if (csProjFile.Find<EmbeddedResource>(zipFileName) != null)
                {
                    csProjFile.Remove<EmbeddedResource>(zipFileName);
                    csProjFile.Save();
                }

                var zipFilePath = input.RootFolder.AppendPath(zipFileName);
                if (fileSystem.FileExists(zipFilePath))
                {
                    Console.WriteLine("Removing obsolete file " + zipFilePath);
                    fileSystem.DeleteFile(zipFilePath);
                }


                return;
            }

            
            var contentFile = FileSystem.Combine(input.RootFolder, zipFileName);
            ConsoleWriter.Write("Creating zip file " + contentFile);
            fileSystem.DeleteFile(contentFile);


            zipService.CreateZipFile(contentFile, file => file.AddFiles(zipRequest));

            if (input.ProjFileFlag.IsEmpty()) return;

            attachZipFileToProjectFile(input, zipFileName);
        }

        public ZipFolderRequest BuildZipRequest(AssemblyPackageInput input, FileSet fileSearch)
        {
            if (fileSearch == null) return null;

            return new ZipFolderRequest{
                FileSet = fileSearch,
                RootDirectory = input.RootFolder,
                ZipDirectory = string.Empty
            };
        }


        private void attachZipFileToProjectFile(AssemblyPackageInput input, string zipFileName)
        {
            // TODO -- determine the csproj file name magically?
            var projectFileName = input.FindCsProjFile();

            var csProjFile = CsProjFile.LoadFrom(projectFileName);
            if (csProjFile.Find<EmbeddedResource>(zipFileName) == null)
            {
                csProjFile.Add<EmbeddedResource>(zipFileName);
                csProjFile.Save();
            }
        }


    }
}