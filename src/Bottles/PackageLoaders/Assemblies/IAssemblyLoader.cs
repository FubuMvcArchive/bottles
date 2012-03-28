using Bottles.Diagnostics;

namespace Bottles.PackageLoaders.Assemblies
{
    /// <summary>
    /// Loads assemblies
    /// </summary>
    public interface IAssemblyLoader
    {
        void ReadPackage(IPackageInfo package, IPackageLog log);
    }
}