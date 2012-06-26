using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.Diagnostics;
using Bottles.Manifest;
using FubuCore;

namespace Bottles.PackageLoaders.Directory
{
    /// <summary>
    /// Used to find packages to be loaded by looking in a directory.
    /// Its not really tied to a solution at all.
    /// 
    /// Renaming this will break BLUE - small break
    /// 
    /// Again these seem to be finders.
    /// </summary>
    public class DirectoryBottleLoader : IBottleLoader
    {
        private readonly string _searchPoint;

        public DirectoryBottleLoader(string searchPoint)
        {
            _searchPoint = searchPoint;
        }

        public IEnumerable<IPackageInfo> Load(IPackageLog log)
        {
            var manifestReader = new BottleManifestReader(new FileSystem(), folder => folder);
            
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