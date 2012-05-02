using System.Reflection;
using Bottles.Exploding;
using FubuCore;

namespace Bottles.PackageLoaders.Assemblies
{
    public static class AssemblyPackageInfoFactory
    {
        public static IPackageInfo CreateFor(Assembly assembly)
        {
            var package = new AssemblyPackageInfo(assembly);

            var exploder = PackageExploder.GetPackageExploder(new FileSystem());
            exploder.ExplodeAssembly(PackageRegistry.GetApplicationDirectory(), assembly, package);

            return package;
        }

        public static IPackageInfo CreateFor(string fileName)
        {
            var assembly = AssemblyLoader.LoadPackageAssemblyFromAppBinPath(fileName);
            return CreateFor(assembly);
        }
    }
}