using Bottles.Commands;
using Bottles.PackageLoaders.Assemblies;

namespace Bottles.Diagnostics
{
    public interface IBottleLogger
    {
        void WriteAssembliesNotFound(AssemblyFiles theAssemblyFiles, PackageManifest manifest, CreateBottleInput theInput, string binFolder);
    }
}