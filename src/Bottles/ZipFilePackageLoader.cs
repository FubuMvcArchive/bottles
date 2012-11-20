using System.Collections.Generic;
using System.Linq;
using Bottles.Diagnostics;
using Bottles.Exploding;
using Bottles.Manifest;
using FubuCore;
using FubuCore.Descriptions;

namespace Bottles
{
    // TODO -- this really needs to be used by FubuMVC.  
    public class ZipFilePackageLoader : IPackageLoader, DescribesItself
    {
        private readonly string _explosionDirectory;
        private readonly IEnumerable<string> _folders;

        public ZipFilePackageLoader(string explosionDirectory, IEnumerable<string> folders)
        {
            _explosionDirectory = explosionDirectory;
            _folders = folders;
        }

        public IEnumerable<IPackageInfo> Load(IPackageLog log)
        {
            BottleExploder exploder = BottleExploder.GetPackageExploder(log);
            var reader = new PackageManifestReader(new FileSystem(), GetContentFolderForPackage);

            return _folders.SelectMany(dir => {
                var explodeDirectory = new ExplodeDirectory
                {
                    DestinationDirectory = _explosionDirectory,
                    BottleDirectory = dir,
                    Log = log
                };

                return exploder.ExplodeDirectory(explodeDirectory);
            }).Select(reader.LoadFromFolder);
        }

        public static string GetContentFolderForPackage(string packageFolder)
        {
            return FileSystem.Combine(packageFolder, BottleFiles.WebContentFolder);
        }

        public void Describe(Description description)
        {
            description.Title = "Load zip files as packages";
            description.ShortDescription = "Looking in folders " + _folders.Join(", ");
        }
    }
}