using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bottles.Diagnostics;
using Bottles.Zipping;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Exploding
{
    public class BottleExploder : IBottleExploder
    {
        private readonly IFileSystem _fileSystem;
        private readonly IBottleExploderLogger _logger;
        private readonly IZipFileService _service;

        public BottleExploder(IZipFileService service, IBottleExploderLogger logger, IFileSystem fileSystem)
        {
            _service = service;
            _logger = logger;
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> ExplodeAllZipsAndReturnPackageDirectories(string applicationDirectory, IBottleLog log)
        {
            LogWriter.Current.Trace("Exploding all the package zip files for the application at " + applicationDirectory);

            return ExplodeDirectory(new ExplodeDirectory(){
                DestinationDirectory = WellKnownFiles.GetExplodedPackagesDirectory(applicationDirectory),
                BottleDirectory = WellKnownFiles.GetApplicationPackagesDirectory(applicationDirectory),
                Log = log
            });
        }

        public IEnumerable<string> ExplodeDirectory(ExplodeDirectory directory)
        {
            string packageFolder = directory.BottleDirectory;
            var fileSet = new FileSet
                          {
                              Include = "*.zip"
                          };

            directory.Log.Trace("Searching for zip files in package directory " + packageFolder);

            var packageFileNames = _fileSystem.FileNamesFor(fileSet, packageFolder);

            return packageFileNames.Select(file =>
            {
                var packageName = Path.GetFileNameWithoutExtension(file);
                var explodedDirectory = FileSystem.Combine(directory.DestinationDirectory, packageName);

                // TODO -- need more logging here. Pass in the log and have it log what happens internally
                var request = new ExplodeRequest{
                    Directory = explodedDirectory,
                    ExplodeAction = () => Explode(file, explodedDirectory, ExplodeOptions.DeleteDestination),
                    GetVersion = () => _service.GetVersion(file),
                    LogSameVersion = () => _logger.WritePackageZipFileWasSameVersionAsExploded(file)
                };


                explode(request);

                return explodedDirectory;
            }).ToList();  // Needs to be evaluated right now.
        }

        //destinationDirectory = var directoryName = BottleFiles.DirectoryForPackageZipFile(applicationDirectory, sourceZipFile);
        public void Explode(string sourceZipFile, string destinationDirectory, ExplodeOptions options)
        {
            _logger.WritePackageZipFileExploded(sourceZipFile, destinationDirectory);
            _service.ExtractTo(sourceZipFile, destinationDirectory, options);
        }

        public void CleanAll(string applicationDirectory)
        {
            ConsoleWriter.Write("Cleaning all exploded packages out of " + applicationDirectory);
            var directory = WellKnownFiles.GetExplodedPackagesDirectory(applicationDirectory);
            clearExplodedDirectories(directory);

            // This is here for legacy installations that may have old exploded packages in bin/fubu-packages
            clearExplodedDirectories(WellKnownFiles.GetApplicationPackagesDirectory(applicationDirectory));
        }

        public string ReadVersion(string directoryName)
        {
            var parts = new[]{
                directoryName,
                WellKnownFiles.VersionFile
            };

            // TODO -- harden?
            if (_fileSystem.FileExists(parts))
            {
                return _fileSystem.ReadStringFromFile(parts);
            }

            return Guid.Empty.ToString();
        }

        public void ExplodeAssembly(string applicationDirectory, Assembly assembly, IBottleInfo bottleInfo)
        {
            var directory = WellKnownFiles.GetDirectoryForExplodedPackage(applicationDirectory, assembly.GetName().Name);

            var request = new ExplodeRequest{
                Directory = directory,
                GetVersion = () => assembly.GetName().Version.ToString(),
                LogSameVersion = () => ConsoleWriter.Write("Assembly {0} has already been 'exploded' onto disk".ToFormat(assembly.GetName().FullName)),
                ExplodeAction = () => explodeAssembly(assembly, directory)
            };

            explode(request);

            _fileSystem.ChildDirectoriesFor(directory).Each(child =>
            {
                var name = Path.GetFileName(child);

                bottleInfo.Files.RegisterFolder(name, child.ToFullPath());
            });
        }

        private void clearExplodedDirectories(string directory)
        {
            _fileSystem.ChildDirectoriesFor(directory).Each(x =>
            {
                _logger.WritePackageDirectoryDeleted(x);
                _fileSystem.DeleteDirectory(x);
            });
        }


        private void explodeAssembly(Assembly assembly, string directory)
        {
            _fileSystem.DeleteDirectory(directory);
            _fileSystem.CreateDirectory(directory);

            assembly.GetManifestResourceNames().Where(WellKnownFiles.IsEmbeddedPackageZipFile).Each(name =>
            {
                var folderName = WellKnownFiles.EmbeddedPackageFolderName(name);
                var stream = assembly.GetManifestResourceStream(name);

                var description = "Resource {0} in Assembly {1}".ToFormat(name, assembly.GetName().FullName);
                var destinationFolder = FileSystem.Combine(directory, folderName);

                _service.ExtractTo(description, stream, destinationFolder);

                var version = assembly.GetName().Version.ToString();
                _fileSystem.WriteStringToFile(FileSystem.Combine(directory, WellKnownFiles.VersionFile), version);
            });
        }

        private void explode(ExplodeRequest request)
        {
            if (_fileSystem.DirectoryExists(request.Directory))
            {
                var packageVersion = request.GetVersion();
                var folderVersion = ReadVersion(request.Directory);

                if (packageVersion == folderVersion)
                {
                    request.LogSameVersion();
                    return;
                }
            }

            request.ExplodeAction();
        }


        public static BottleExploder GetPackageExploder(IFileSystem fileSystem)
        {
            return new BottleExploder(new ZipFileService(fileSystem), new BottleExploderLogger(text => LogWriter.Current.Trace(text)), fileSystem);
        }

        public static BottleExploder GetPackageExploder(IBottleLog log)
        {
            var fileSystem = new FileSystem();
            return new BottleExploder(new ZipFileService(fileSystem), new BottleExploderLogger(text => log.Trace(text)), fileSystem);
        }




        #region Nested type: ExplodeRequest

        public class ExplodeRequest
        {
            public Func<string> GetVersion { get; set; }
            public string Directory { get; set; }
            public Action ExplodeAction { get; set; }
            public Action LogSameVersion { get; set; }
        }

        #endregion
    }

    
}