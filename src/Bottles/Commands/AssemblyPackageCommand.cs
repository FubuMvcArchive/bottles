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

        [Description("Name of the csproj file.  If set, this command attempts to add the zip files as embedded resources")]
        public string ProjFileFlag { get; set; }

        [Description("Previews which files will be added to the assembly bottle")]
        public bool PreviewFlag { get; set; }
    }

    // TODO -- do something that tests this
    [CommandDescription("Bundle up the content and data files for a self contained assembly package", Name = "assembly-pak")]
    public class AssemblyPackageCommand : FubuCommand<AssemblyPackageInput>
    {
        IFileSystem fileSystem = new FileSystem();

        public override bool Execute(AssemblyPackageInput input)
        {
            input.RootFolder = new AliasService().GetFolderForAlias(input.RootFolder);

            if (input.PreviewFlag)
            {
                var manifest = fileSystem.LoadPackageManifestFrom(input.RootFolder);
                if (manifest == null)
                {
                    Console.WriteLine("Package manifest file {0} was not found", input.RootFolder.AppendPath(PackageManifest.FILE));
                    return false;
                }

                previewFiles(input, BottleFiles.WebContentFolder, manifest.ContentFileSet);
                previewFiles(input, BottleFiles.DataFolder, manifest.DataFileSet);
                previewFiles(input, BottleFiles.ConfigFolder, manifest.ConfigFileSet);


                return true;
            }

            var zipService = new ZipFileService(fileSystem);

            // TODO -- this is where it would be valuable to start generalizing the file set
            // storage within Package Manifest
            createZipFile(input, "WebContent", zipService, m => m.ContentFileSet);
            createZipFile(input, "Data", zipService, m => m.DataFileSet);
            createZipFile(input, "Config", zipService, m => m.ConfigFileSet);


            return true;
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

        private void createZipFile(AssemblyPackageInput input, string childFolderName, ZipFileService zipService, Func<PackageManifest, FileSet> fileSource)
        {
            var zipRequest = BuildZipRequest(input, childFolderName, fileSource);
            if (zipRequest == null)
            {
                ConsoleWriter.Write("No content for " + childFolderName);

                return;
            }

            var zipFileName = "pak-{0}.zip".ToFormat(childFolderName);
            var contentFile = FileSystem.Combine(input.RootFolder, zipFileName);
            ConsoleWriter.Write("Creating zip file " + contentFile);
            fileSystem.DeleteFile(contentFile);


            zipService.CreateZipFile(contentFile, file => file.AddFiles(zipRequest));

            if (input.ProjFileFlag.IsEmpty()) return;

            attachZipFileToProjectFile(input, zipFileName);
        }

        public ZipFolderRequest BuildZipRequest(AssemblyPackageInput input, string childFolderName, Func<PackageManifest, FileSet> fileSource)
        {
            var contentDirectory = FileSystem.Combine(input.RootFolder, childFolderName);

            var manifest = fileSystem.LoadPackageManifestFrom(input.RootFolder);
            if (manifest != null)
            {
                var files = fileSource(manifest);
                if (files == null) return null;

                return new ZipFolderRequest{
                    FileSet = files,
                    RootDirectory = input.RootFolder,
                    ZipDirectory = string.Empty
                };
            }

            if (!fileSystem.DirectoryExists(contentDirectory)) return null;

            return new ZipFolderRequest()
                   {
                       FileSet = new FileSet() { DeepSearch = true, Include = "*.*" },
                       RootDirectory = contentDirectory,
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