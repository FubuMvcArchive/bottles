using System.Reflection;
using Bottles.Exploding;
using FubuCore;

namespace Bottles.PackageLoaders.Assemblies
{
    public static class AssemblyBottleInfoFactory
    {
        public static IBottleInfo CreateFor(Assembly assembly)
        {
            var package = new AssemblyBottleInfo(assembly);

            var exploder = BottleExploder.GetPackageExploder(new FileSystem());
            exploder.ExplodeAssembly(BottleRegistry.GetApplicationDirectory(), assembly, package);

            return package;
        }

        public static IBottleInfo CreateFor(string fileName)
        {
            var assembly = AssemblyLoader.LoadPackageAssemblyFromAppBinPath(fileName);
            return CreateFor(assembly);
        }
    }
}