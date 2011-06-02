using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;
using Bottles;

namespace Bottles
{
    public class PackageManifestReader : IPackageManifestReader
    {
        private readonly IFileSystem _fileSystem;
        private readonly Func<string, string> _getContentFolderFromPackageFolder;

        public PackageManifestReader(IFileSystem fileSystem, Func<string, string> getContentFolderFromPackageFolder)
        {
            _fileSystem = fileSystem;
            _getContentFolderFromPackageFolder = getContentFolderFromPackageFolder;
        }

        public IPackageInfo LoadFromFolder(string packageDirectory)
        {
            packageDirectory = packageDirectory.ToFullPath();

            var manifest = _fileSystem.LoadFromFile<PackageManifest>(packageDirectory, PackageManifest.FILE);
            var package = new PackageInfo(manifest.Name){
                Description = "{0} ({1})".ToFormat(manifest.Name, packageDirectory),
            };


            // Right here, this needs to be different
            package.RegisterFolder(BottleFiles.WebContentFolder, _getContentFolderFromPackageFolder(packageDirectory));
            package.RegisterFolder(BottleFiles.DataFolder, FileSystem.Combine(packageDirectory, BottleFiles.DataFolder));
            package.RegisterFolder(BottleFiles.ConfigFolder, FileSystem.Combine(packageDirectory, BottleFiles.ConfigFolder));

            var binPath = FileSystem.Combine(packageDirectory, "bin");
            var debugPath = FileSystem.Combine(binPath, "debug");
            if (_fileSystem.DirectoryExists(debugPath))
            {
                binPath = debugPath;
            }

            //REVIEW: I feel this whole section is left-hand / right-hand code
            package.Role = manifest.Role;

            var assemblyPaths = findCandidateAssemblyFiles(binPath);
            assemblyPaths.Each(path =>
            {
                var assemblyName = Path.GetFileNameWithoutExtension(path);
                if (manifest.Assemblies.Contains(assemblyName))
                {
                    package.RegisterAssemblyLocation(assemblyName, path);
                }
            });

            return package;
        }

        private static IEnumerable<string> findCandidateAssemblyFiles(string binPath)
        {
            if (!Directory.Exists(binPath))
            {
                return new string[0];
            }

            return Directory.GetFiles(binPath).Where(IsPotentiallyAnAssembly);
        }

        public static bool IsPotentiallyAnAssembly(string file)
        {
            var extension = Path.GetExtension(file);
            return extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".dll", StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return "Package Manifest Reader (Development Mode)";
        }
    }
}