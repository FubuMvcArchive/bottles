using System.Collections.Generic;
using System.ComponentModel;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Commands
{
    public class AssembliesInput
    {
        public AssembliesInput()
        {
            Target = CompileTarget.Debug.ToString();
        }

        [Description("Add, remove, or list the assemblies for this manifest")]
        public AssembliesCommandMode Mode { get; set; }

        [Description("The package or application directory")]
        public string Directory { get; set; }

        [Description("Overrides the name of the manifest file if it's not the default .package-manifest or .fubu-manifest")]
        [FlagAlias("file", 'f')]
        public string FileNameFlag { get; set; }

        [Description("Add or removes the named assembly")]
        public string AssemblyName { get; set; }

        [Description("Opens the manifest file in your editor")]
        public bool OpenFlag { get; set; }

        [Description("Choose the compilation target for any assemblies.  Default is Debug")]
        public string Target { get; set; }

        [IgnoreOnCommandLine]
        public PackageManifest Manifest { get; set;}

        [IgnoreOnCommandLine]
        public string BinariesFolder { get; set;}



        public void FindManifestAndBinaryFolders(IFileSystem fileSystem)
        {
            BinariesFolder = fileSystem.FindBinaryDirectory(Directory, Target);

            Manifest = fileSystem.TryFindManifest(Directory, FileNameFlag) ??
                       fileSystem.LoadPackageManifestFrom(Directory);
        }

        public void Save(IFileSystem fileSystem)
        {
            fileSystem.PersistToFile(Manifest, Manifest.ManifestFileName);
        }

        public void RemoveAssemblies(IFileSystem fileSystem)
        {
            if (AssemblyName.IsNotEmpty())
            {
                Manifest.RemoveAssembly(AssemblyName);
            }
            else
            {
                Manifest.RemoveAllAssemblies();
            }
            
            Save(fileSystem);
        }

        public void AddAssemblies(IFileSystem fileSystem)
        {
            if (AssemblyName.IsNotEmpty())
            {
                Manifest.AddAssembly(AssemblyName);
            }
            else
            {
                fileSystem.FindAssemblyNames(BinariesFolder).Each(name => Manifest.AddAssembly(name));
            }
            
            Save(fileSystem);
        }
    }
}