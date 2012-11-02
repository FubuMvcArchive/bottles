using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.Diagnostics;
using Bottles.Manifest;
using FubuCore;
using FubuCore.Descriptions;

namespace Bottles.PackageLoaders.Directory
{
    /// <summary>
    /// Used to find packages to be loaded by looking in a directory.
    /// </summary>
    public class DirectoryPackageLoader : IPackageLoader, DescribesItself
    {
        private readonly string _searchPoint;

        public DirectoryPackageLoader(string searchPoint)
        {
            _searchPoint = searchPoint;
        }

        public IEnumerable<IPackageInfo> Load(IPackageLog log)
        {
            var manifestReader = new PackageManifestReader(new FileSystem(), folder => folder);
            
            var pis = PackageManifest.FindManifestFilesInDirectory(_searchPoint)
                .Select(Path.GetDirectoryName)
                .Select(manifestReader.LoadFromFolder);

            var filtered = pis.Where(pi=>BottleRoles.Module.Equals(pi.Role));

            LogWriter.Current.PrintHorizontalLine();
            LogWriter.Current.Trace("Solution Package Loader found:");
            LogWriter.Current.Indent(() =>
            {
                filtered.Each(p => LogWriter.Current.Trace(p.Name));
            });

            LogWriter.Current.PrintHorizontalLine();

            return filtered;
        }

        public void Describe(Description description)
        {
            description.Title = "Directory Package Loader";
            description.ShortDescription = "Scans directory {0} for packages".ToFormat(_searchPoint);
        }
    }
}