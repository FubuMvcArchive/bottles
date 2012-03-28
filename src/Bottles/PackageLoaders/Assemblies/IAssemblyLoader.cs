using Bottles.Diagnostics;

namespace Bottles.PackageLoaders.Assemblies
{
    public interface IAssemblyLoader
    {
        void ReadPackage(IPackageInfo package, IPackageLog log);
    }
}