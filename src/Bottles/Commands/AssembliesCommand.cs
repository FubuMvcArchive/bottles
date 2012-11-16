using Bottles.Creation;
using FubuCore;
using FubuCore.CommandLine;
using System.Collections.Generic;
using System.Linq;

namespace Bottles.Commands
{
    public enum AssembliesCommandMode
    {
        add,
        remove,
        list
    }

    [CommandDescription("Adds assemblies to a given manifest")]
    public class AssembliesCommand : FubuCommand<AssembliesInput>
    {
        public AssembliesCommand()
        {
            Usage("Remove or adds all assemblies to the manifest file").Arguments(x => x.Mode, x => x.Directory);
            Usage("Removes or adds a single assembly name to the manifest file").Arguments(x => x.Mode, x => x.Directory,
                                                                                           x => x.AssemblyName);


        }

        public override bool Execute(AssembliesInput input)
        {
            input.Directory = new AliasService().GetFolderForAlias(input.Directory);

            var fileSystem = new FileSystem();
            input.FindManifestAndBinaryFolders(fileSystem);
            
            

            return Execute(fileSystem, input);
        }

        private bool Execute(IFileSystem fileSystem, AssembliesInput input)
        {
            // return false if manifest does not exist
            if (input.Manifest == null)
            {
                throw new CommandFailureException("Could not find a PackageManifest in the directory " + input.Directory);
            }


            switch (input.Mode)
            {
                case AssembliesCommandMode.add:
                    input.AddAssemblies(fileSystem);
                    break;

                case AssembliesCommandMode.remove:
                    input.RemoveAssemblies(fileSystem);
                    break;

                case AssembliesCommandMode.list:
                    ListAssemblies(fileSystem, input);
                    break;
            }

            if (input.OpenFlag)
            {
                fileSystem.LaunchEditor(input.Manifest.ManifestFileName);
            }

            return true;
        }

        public static void ListAssemblies(IFileSystem fileSystem, AssembliesInput input)
        {
            ConsoleWriter.Write("Assemblies referenced in {0} are:", input.Manifest.ManifestFileName);

            input.Manifest.Assemblies.Each(name => ConsoleWriter.Write(" * " + name));

            ConsoleWriter.Line();
            ConsoleWriter.Write("Assemblies at {0} not referenced in the manifest:");

            fileSystem
                .FindAssemblyNames(input.BinariesFolder)
                .Where(x => !input.Manifest.Assemblies.Contains(x))
                .Each(x => ConsoleWriter.Write(" * " + x));
        }
    }
}