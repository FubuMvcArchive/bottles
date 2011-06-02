using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Bottles.Deployment.Bootstrapping;
using Bottles.Deployment.Runtime;
using FubuCore;
using FubuCore.CommandLine;
using System.Collections.Generic;

namespace Bottles.Deployment.Commands
{
    public enum ListMode
    {
        manifests,
        directives,
        recipes,
        profiles,
        bottles,
        hosts,
        all
    }

    public class ListInput
    {
        public ListInput()
        {
            Mode = ListMode.manifests;
        }

        [Description("What to list")]
        [RequiredUsage("mode")]
        public ListMode Mode { get; set; }
        
        
        [Description("Where to scan")]
        public string PointFlag { get; set; }

        [Description("The directory where the deployment settings are stored")]
        public string DeploymentFlag { get; set; }

        
        public string PointToScan()
        {
            var x = PointFlag ?? ".";
            return x.ToFullPath();
        }
    }

    [CommandDescription("Lists all discovered manifests", Name="list")]
    [Usage("default", "List manifests")]
    [Usage("mode", "list something specific")]
    public class ListCommand : FubuCommand<ListInput>
    {
        public override bool Execute(ListInput input)
        {
            var point = input.PointToScan();
            
            


            var system = new FileSystem();

            switch (input.Mode)
            {

                case ListMode.directives:
                    writeDirectives(input, system);
                    break;

                case ListMode.bottles:
                    writeBottles(input, system);
                    break;

                case ListMode.manifests:
                    writePackages(input, point, system);
                    break;

                case ListMode.recipes:
                    writeRecipes(input, system);
                    break;

                case ListMode.hosts:
                    writeHosts(input, system);
                    break;

                case ListMode.profiles:
                    writeProfiles(input, system);
                    break;

                case ListMode.all:
                    write("Bottles", () => writeBottles(input, system));
                    write("Package Manifests", () => writePackages(input, point, system));
                    write("Recipes", () => writeRecipes(input, system));
                    write("Hosts", () => writeHosts(input, system));
                    write("Profiles", () => writeProfiles(input, system));
                    write("Directive Types", () => writeDirectives(input, system));

                    break;
            }

            


            return true;
        }

        private void writeHosts(ListInput input, FileSystem system)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);
            system.ChildDirectoriesFor(settings.RecipesDirectory).Each(x =>
            {
                var recipeName = Path.GetFileNameWithoutExtension(x);
                system.FindFiles(x, new FileSet(){
                    Include = "*.host"
                }).Each(file =>
                {
                    var hostName = Path.GetFileNameWithoutExtension(file);
                    ConsoleWriter.Write(recipeName + " / " + hostName);
                });
            });
        }

        private void writeDirectives(ListInput input, FileSystem system)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);
            var container = DeploymentBootstrapper.Bootstrap(settings);
            container.GetInstance<IDirectiveTypeRegistry>().DirectiveTypes().Each(type =>
            {
                ConsoleWriter.Write(type.Name);
            });
        }

        private void writeProfiles(ListInput input, FileSystem system)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);
            system.FindFiles(settings.ProfilesDirectory, new FileSet(){
                Include = "*.*"
            })
                .Select(Path.GetFileNameWithoutExtension)
                .Each(ConsoleWriter.Write);
        }

        private void writeRecipes(ListInput input, FileSystem system)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);
            system.ChildDirectoriesFor(settings.RecipesDirectory).Each(x =>
            {
                ConsoleWriter.Write(Path.GetFileNameWithoutExtension(x));
            });
        }

        private void write(string title, Action action)
        {
            ConsoleWriter.Write(ConsoleColor.Cyan, title);
            action();
            ConsoleWriter.PrintHorizontalLine();
            ConsoleWriter.Line();
        }

        private void writeBottles(ListInput input, IFileSystem fileSystem)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);
            var bottlesDir = settings.BottlesDirectory;

            ConsoleWriter.Line();
            ConsoleWriter.Write(ConsoleColor.White, "Bottles at " + bottlesDir);
            fileSystem.FindFiles(bottlesDir, new FileSet(){
                Include = "*.*"
            }).OrderBy(x => x).Each(x => ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, 2, x));
        }



        private void writePackages(ListInput input, string point, FileSystem system)
        {
            ConsoleWriter.Write("Looking for manifests starting from: {0}", point);

            var fileSet = new FileSet()
                          {
                              DeepSearch = true,
                              Include = PackageManifest.FILE
                          };
            var manifests = system.FileNamesFor(fileSet, input.PointToScan())
                .Select(filename => Path.GetDirectoryName(filename));

            foreach (var manifestDir in manifests)
            {
                ConsoleWriter.PrintHorizontalLine();
                var shorty = manifestDir.Remove(0, point.Length);
                ConsoleWriter.Write("Found manifest at: {0}", shorty);
                var m = system.LoadPackageManifestFrom(manifestDir);
                ConsoleWriter.Write("Name: {0}", m.Name);
                ConsoleWriter.Write("Role: {0}", m.Role);
            }
        }
    }
}