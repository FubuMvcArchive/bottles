using Bottles.Creation;
using Bottles.PackageLoaders.Assemblies;

namespace Bottles.Diagnostics
{
    public interface IPackageLogger
    {
        void WriteAssembliesNotFound(AssemblyFiles theAssemblyFiles, PackageManifest manifest, CreateBottleInput theInput, string binFolder);
    }
}