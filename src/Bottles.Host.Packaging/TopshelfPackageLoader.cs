using System;
using System.Collections.Generic;
using System.Linq;
using Bottles.Diagnostics;
using Bottles.Exploding;
using Bottles.Manifest;
using FubuCore;

namespace Bottles.Host.Packaging
{
    public class TopshelfPackageLoader : 
        IPackageLoader
    {
        public static readonly string TopshelfPackagesFolder = "topshelf-packages";
        public static readonly string TopshelfContentFolder = "topshelf-content";

        private readonly IBottleExploder _exploder;
        private readonly BottleManifestReader _reader;

        public TopshelfPackageLoader(IBottleExploder exploder)
        {
            _exploder = exploder;

            _reader = new BottleManifestReader(new FileSystem(), getContentFolderForPackage);
        }

        public IEnumerable<IPackageInfo> Load(IPackageLog log)
        {
            return getPackageDirectories().SelectMany(dir=>
            {
                return _exploder.ExplodeDirectory(new ExplodeDirectory{
                    DestinationDirectory = getExplodedPackagesDirectory(),
                    BottleDirectory = dir,
                    Log = log
                });
            }).Select(dir=>_reader.LoadFromFolder(dir));
        }

        private static string getContentFolderForPackage(string packageFolder)
        {
            return packageFolder.AppendPath(BottleFiles.WebContentFolder);
        }

        private static string getExplodedPackagesDirectory()
        {
            return getApplicationPath().AppendPath(TopshelfContentFolder);
        }

        private static IEnumerable<string> getPackageDirectories()
        {
            yield return getApplicationPath().AppendPath(TopshelfPackagesFolder);
            yield return getApplicationPath().AppendPath(TopshelfContentFolder);
        }

        private static string getApplicationPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}