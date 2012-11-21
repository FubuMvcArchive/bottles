using System;
using System.ComponentModel;
using System.Xml;
using Bottles.Zipping;
using FubuCore;
using FubuCore.CommandLine;
using System.Linq;
using System.Collections.Generic;

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
    }

    [CommandDescription("Bundle up the content and data files for a self contained assembly package", Name = "assembly-pak")]
    public class AssemblyPackageCommand : FubuCommand<AssemblyPackageInput>
    {
        IFileSystem fileSystem = new FileSystem();

        public override bool Execute(AssemblyPackageInput input)
        {
            input.RootFolder = new AliasService().GetFolderForAlias(input.RootFolder);

            determineMissingCsProjFile(input);

            if (input.PreviewFlag)
            {
                return displayPreview(input);
            }

            var zipService = new ZipFileService(fileSystem);

            var manifest = fileSystem.LoadPackageManifestFrom(input.RootFolder);
            if (manifest == null)
            {
                Console.WriteLine("No PackageManifest found, using defaults instead");
                manifest = new PackageManifest();
                manifest.SetRole(BottleRoles.Module);


                Console.WriteLine("WebContent:  " + manifest.ContentFileSet);
            }

            // TODO -- automatically filter out /data and /config
            createZipFile(input, BottleFiles.WebContentFolder, zipService, manifest.ContentFileSet);
            createZipFile(input, BottleFiles.DataFolder, zipService, BottleFiles.DataFiles);
            createZipFile(input, BottleFiles.ConfigFolder, zipService, BottleFiles.ConfigFiles);


            return true;
        }

        private bool displayPreview(AssemblyPackageInput input)
        {
            var manifest = fileSystem.LoadPackageManifestFrom(input.RootFolder);
            if (manifest == null)
            {
                Console.WriteLine("Package manifest file {0} was not found", input.RootFolder.AppendPath(PackageManifest.FILE));
                return false;
            }

            // TODO -- automatically filter out /data and /config
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
                    Console.WriteLine("Found 1 csproj file");
                    Console.WriteLine("Using " + files.Single());
                    input.ProjFileFlag = files.Single().ToFullPath();
                }

                if (files.Count() > 1)
                {
                    Console.WriteLine(
                        "Found more than one *.csproj file in this directory.  You'll need to specify the --proj-file flag");
                }
            }
        }

        private void previewFiles(AssemblyPackageInput input, string folder, FileSet fileSearch)
        {
            if (fileSearch == null)
            {
                Console.WriteLine("{0}:  No files", folder);
                return;
            }

            var fileSystem = new FileSystem();
            var contentFolder = input.RootFolder.AppendPath(folder);

            var files = fileSystem.DirectoryExists(contentFolder) ? fileSystem.FindFiles(contentFolder, fileSearch) : fileSystem.FindFiles(input.RootFolder, fileSearch);

            if (files.Any())
            {
                Console.WriteLine("{0}: {1} file(s)", folder, files.Count());
                files.Each(f => Console.WriteLine(f));
            }
            else
            {
                Console.WriteLine("{0}:  No files", folder);
            }

            Console.WriteLine();
        }

        private void createZipFile(AssemblyPackageInput input, string childFolderName, ZipFileService zipService, FileSet fileSearch)
        {
            var zipRequest = BuildZipRequest(input, fileSearch);
            if (zipRequest == null)
            {
                ConsoleWriter.Write("No content for " + childFolderName);

                return;
            }

            // Hack, but it makes /data and data/*.* work
            if (fileSystem.DirectoryExists(input.RootFolder.AppendPath(childFolderName)) && zipRequest.FileSet.Include.StartsWith(childFolderName, StringComparison.InvariantCultureIgnoreCase))
            {
                zipRequest.FileSet = FileSet.Deep("*.*");
                zipRequest.RootDirectory = zipRequest.RootDirectory.AppendPath(childFolderName);
            }

            var zipFileName = "pak-{0}.zip".ToFormat(childFolderName);
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
            var document = new XmlDocument();
            var projectFileName = FileSystem.Combine(input.RootFolder, input.ProjFileFlag);
            document.Load(projectFileName);

            //var search = "//ItemGroup/EmbeddedResource[@Include='{0}']".ToFormat(zipFileName);
            //if (document.DocumentElement.SelectSingleNode(search, new XmlNamespaceManager(document.NameTable)) == null)
            if (document.DocumentElement.OuterXml.Contains(zipFileName))
            {
                ConsoleWriter.Write("The file {0} is already embedded in project {1}".ToFormat(zipFileName, projectFileName));
                return;
            }

            ConsoleWriter.Write("Adding the ItemGroup / Embedded Resource for {0} to {1}".ToFormat(zipFileName,
                                                                                                   projectFileName));
            var node = document.CreateNode(XmlNodeType.Element, "ItemGroup", document.DocumentElement.NamespaceURI);
            var element = document.CreateNode(XmlNodeType.Element, "EmbeddedResource", document.DocumentElement.NamespaceURI);
            var attribute = document.CreateAttribute("Include");
            attribute.Value = zipFileName;
            element.Attributes.Append(attribute);
            node.AppendChild(element);
            document.DocumentElement.AppendChild(node);

            document.Save(projectFileName);
        }
    }
}