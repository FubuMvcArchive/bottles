using System.Collections.Generic;
using System.Linq;
using Bottles.Diagnostics;
using Bottles.Exploding;
using FubuCore;

namespace Bottles.Host.Packaging
{
    public class TopshelfPackageLoader : 
        IPackageLoader
    {
        private readonly IPackageExploder _exploder;
        private readonly PackageManifestReader _reader;

        public TopshelfPackageLoader(IPackageExploder exploder)
        {
            _exploder = exploder;

            _reader = new PackageManifestReader(new FileSystem(), GetContentFolderForPackage);
        }

        public IEnumerable<IPackageInfo> Load(IPackageLog log)
        {
            return TopshelfPackageFacility.GetPackageDirectories().SelectMany(dir=>
            {
                return _exploder.ExplodeDirectory(new ExplodeDirectory{
                    DestinationDirectory = TopshelfPackageFacility.GetExplodedPackagesDirectory(),
                    PackageDirectory = dir,
                    Log = log
                });
            }).Select(dir=>_reader.LoadFromFolder(dir));
        }

        public static string GetContentFolderForPackage(string packageFolder)
        {
            return FileSystem.Combine(packageFolder, BottleFiles.WebContentFolder);
        }
    }
}