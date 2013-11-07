using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Bottles.PackageLoaders.LinkedFolders;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Commands
{
    public class LinkInput
    {
        [Description("The physical folder (or valid alias) of the main application")]
        public string AppFolder { get; set; }

        [Description("The physical folder (or valid alias) of a bottle")]
        public string BottleFolder { get; set; }

        [Description("Remove the package folder link from the application")]
        public bool RemoveFlag { get; set; }

        [Description("Remove all links from an application manifest file")]
        public bool CleanAllFlag { get; set; }

        [Description("Create a 'remote' link that will run the service or app at the BottleFolder in a separate AppDomain")]
        public bool RemoteFlag { get; set; }

        [Description("Explicitly specify the name of the Bottles bootstrapper class for a remote link")]
        public string BootstrapperFlag { get; set; }

        [Description("Specify the name of the configuration file to use for a remote link")]
        [FlagAlias("config-file",'f')]
        public string ConfigFileFlag { get; set; }


        public string RelativePathOfPackage()
        {
            var pkg = Path.GetFullPath(BottleFolder);
            var app = Path.GetFullPath(AppFolder);

            return pkg.PathRelativeTo(app);
        }
    }

    [CommandDescription("Links a package folder to an application folder in development mode")]
    public class LinkCommand : FubuCommand<LinkInput>
    {
        public LinkCommand()
        {
            Usage("List the current links for the application").Arguments(x => x.AppFolder).ValidFlags();
            Usage("Create a new link for the application to the package").Arguments(x => x.AppFolder, x => x.BottleFolder).ValidFlags();
            Usage("Remove any existing link for the application to the package").Arguments(x => x.AppFolder, x => x.BottleFolder).ValidFlags(x => x.RemoveFlag);
            Usage("Remove any and all existing links from the application to any package folder").Arguments(x => x.AppFolder).ValidFlags(x => x.CleanAllFlag);
        }

        readonly ILinksService _links = new LinksService(new FileSystem());

        public override bool Execute(LinkInput input)
        {
            input.AppFolder = new AliasService().GetFolderForAlias(input.AppFolder);
            input.BottleFolder = new AliasService().GetFolderForAlias(input.BottleFolder);


            Execute(input, _links);
            return true;
        }

        public void Execute(LinkInput input, ILinksService links)
        {
            var manifest = links.GetLinkManifest(input.AppFolder);

            if (input.CleanAllFlag && links.LinkManifestExists(input.AppFolder))
            {
                manifest.RemoveAllLinkedFolders();

                links.Save(manifest, input.AppFolder);

                ConsoleWriter.Write("Removed all package links from the manifest file for " + input.AppFolder);

                listCurrentLinks(input.AppFolder, manifest);

                return;
            }



            if (input.BottleFolder.IsNotEmpty())
            {
                updateManifest(links, input, manifest);
            }
            else
            {
                listCurrentLinks(input.AppFolder, manifest);
            }

        }

        private static void listCurrentLinks(string appFolder, LinkManifest manifest)
        {
            if (manifest.LinkedFolders.Any())
            {
                ConsoleWriter.Write("  Links for " + appFolder);
                manifest.LinkedFolders.Each(x => { ConsoleWriter.Write("    " + x); });
            }
            else
            {
                ConsoleWriter.Write("  No package links for " + appFolder);
            }

            if (manifest.RemoteLinks.Any())
            {
                ConsoleWriter.Write("  Remote Links for " + appFolder);
                manifest.RemoteLinks.Each(x => { ConsoleWriter.Write("    " + x); });
            }
        }

        private void updateManifest(ILinksService links, LinkInput input, LinkManifest manifest)
        {
            if (input.RemoveFlag)
            {
                manifest.RemoveLink(input.RelativePathOfPackage());
                ConsoleWriter.Write("Folder {0} was removed from the application at {1}", input.BottleFolder,
                                    input.AppFolder);
            }
            else
            {
                if (input.RemoteFlag)
                {
                    
                    var link = addRemoteLink(input, manifest);
                    Console.WriteLine("Setting remote link " + link);
                }
                else
                {
                    add(input, manifest);
                }

                
            }

            links.Save(manifest, input.AppFolder);
        }

        private RemoteLink addRemoteLink(LinkInput input, LinkManifest manifest)
        {
            return manifest.AddRemoteLink(input);
        }

        private static void add(LinkInput input, LinkManifest manifest)
        {
            var wasAdded = manifest.AddLink(input.RelativePathOfPackage());
            var msg = wasAdded
                          ? "Folder {0} was added to the application at {1}"
                          : "Folder {0} is already included in the application at {1}";

            var system = new FileSystem();

            var bottleFolder = input.BottleFolder;

            if (!system.FileExists(bottleFolder.AppendPath(PackageManifest.FILE)))
            {
                var name = GuessAssemblyNameForFolder(bottleFolder, system);

                ConsoleWriter.Write(ConsoleColor.Green, "Initializing a new {0} file at {1}".ToFormat(PackageManifest.FILE, bottleFolder));
                
                new InitCommand().Execute(new InitInput
                {
                    Path = bottleFolder,
                    Name = name
                });
            }

            ConsoleWriter.Write(msg, bottleFolder, input.AppFolder);
        }

        public static string GuessAssemblyNameForFolder(string bottleFolder, FileSystem system)
        {
// First guess of the bottle name is by the folder
            var name = Path.GetFileNameWithoutExtension(bottleFolder);

            // Second guess is a csproj file if one exists
            var files = system.FindFiles(bottleFolder, FileSet.Shallow("*.csproj"));
            if (files.Count() == 1)
            {
                name = Path.GetFileNameWithoutExtension(files.Single());
            }
            return name;
        }
    }
}