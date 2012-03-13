using Bottles.Assemblies;
using Bottles.Creation;

namespace Bottles.Diagnostics
{
    public interface IPackageLogger
    {
        void WriteAssembliesNotFound(AssemblyFiles theAssemblyFiles, PackageManifest manifest, CreateBottleInput theInput, string binFolder);
    }
}