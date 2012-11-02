using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;

namespace Bottles.Manifest
{
    public class PackageManifestReader : IPackageManifestReader
    {
        private readonly IFileSystem _fileSystem;
        private readonly Func<string, string> _getContentFolderFromBottleFolder;

        public PackageManifestReader(IFileSystem fileSystem, Func<string, string> getContentFolderFromBottleFolder)
        {
            _fileSystem = fileSystem;
            _getContentFolderFromBottleFolder = getContentFolderFromBottleFolder;
        }

        public PackageManifest LoadFromStream(Stream stream)
        {
            var manifest = _fileSystem.LoadFromStream<PackageManifest>(stream);
            return manifest;
        }

        public IPackageInfo LoadFromFolder(string packageDirectory)
        {
            packageDirectory = packageDirectory.ToFullPath();

            var manifest = _fileSystem.LoadFromFile<PackageManifest>(packageDirectory, PackageManifest.FILE);

            var package = new PackageInfo(manifest){
                Description = "{0} ({1})".ToFormat(manifest.Name, packageDirectory),
                Dependencies = manifest.Dependencies
            };

            // Right here, this needs to be different
            registerFolders(packageDirectory, package);

            var binPath = determineBinPath(packageDirectory);

            readAssemblyPaths(manifest, package, binPath);

            return package;
        }

        private string determineBinPath(string packageDirectory)
        {
            var binPath = FileSystem.Combine(packageDirectory, "bin");
            var debugPath = FileSystem.Combine(binPath, "debug");
            if (_fileSystem.DirectoryExists(debugPath))
            {
                binPath = debugPath;
            }
            return binPath;
        }

        private void registerFolders(string bottleDirectory, PackageInfo package)
        {
            package.RegisterFolder(BottleFiles.WebContentFolder, _getContentFolderFromBottleFolder(bottleDirectory));
            package.RegisterFolder(BottleFiles.DataFolder, FileSystem.Combine(bottleDirectory, BottleFiles.DataFolder));
            package.RegisterFolder(BottleFiles.ConfigFolder, FileSystem.Combine(bottleDirectory, BottleFiles.ConfigFolder));
        }

        private void readAssemblyPaths(PackageManifest manifest, PackageInfo package, string binPath)
        {
            var assemblyPaths = findCandidateAssemblyFiles(binPath);
            assemblyPaths.Each(path =>
            {
                var assemblyName = Path.GetFileNameWithoutExtension(path);
                if (manifest.Assemblies.Contains(assemblyName))
                {
                    package.RegisterAssemblyLocation(assemblyName, path);
                }
            });
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
            return "Bottle Manifest Reader (Development Mode)";
        }
    }
}