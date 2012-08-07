using System;
using System.Reflection;
using Bottles.Exploding;
using FubuCore;

namespace Bottles.PackageLoaders.Assemblies
{
    [MarkedForTermination("Need to get this shit encapsulated back into AssemblyPackageInfo where it belongs")]
    public static class AssemblyPackageInfoFactory
    {
        public static IPackageInfo CreateFor(Assembly assembly)
        {
            var package = new AssemblyPackageInfo(assembly);

            var exploder = BottleExploder.GetPackageExploder(new FileSystem());
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