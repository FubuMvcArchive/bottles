using System;
using System.IO;
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

            exploder.ExplodeAssembly(PackageRegistry.GetApplicationDirectory(), assembly, package.Files);

            return package;
        }

        // TODO -- remove duplication with AssemblyLoader
        public static IPackageInfo CreateFor(string fileName)
        {
            var assembly = loadPackageAssemblyFromAppBinPath(fileName);
            return CreateFor(assembly);
        }


        private static Assembly loadPackageAssemblyFromAppBinPath(string file)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(file);
            var appBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath ?? AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (!Path.GetDirectoryName(file).EqualsIgnoreCase(appBinPath))
            {
                var destFileName = FileSystem.Combine(appBinPath, Path.GetFileName(file));
                if (shouldUpdateFile(file, destFileName))
                {
                    File.Copy(file, destFileName, true);
                }
            }
            return Assembly.Load(assemblyName);
        }

        private static bool shouldUpdateFile(string source, string destination)
        {
            return !File.Exists(destination) || File.GetLastWriteTimeUtc(source) > File.GetLastWriteTimeUtc(destination);
        }
    }
}