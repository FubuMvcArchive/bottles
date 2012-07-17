using Bottles.Diagnostics;

namespace Bottles.PackageLoaders.Assemblies
{
    /// <summary>
    /// Loads assemblies
    /// </summary>
    public interface IAssemblyLoader
    {
        void ReadPackage(IBottleInfo bottle, IBottleLog log);
    }
}