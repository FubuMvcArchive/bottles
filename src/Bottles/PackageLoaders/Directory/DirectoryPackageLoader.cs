using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.PackageLoaders
{
    /// <summary>
    /// Used to find packages to be loaded by looking in a directory.
    /// Its not really tied to a solution at all.
    /// 
    /// Renaming this will break BLUE - small break
    /// 
    /// Again these seem to be finders.
    /// </summary>
    public class DirectoryPackageLoader : IPackageLoader
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
    }
}