using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.Diagnostics;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles
{
    public class SolutionDirectoryPackageLoader : IPackageLoader
    {
        private readonly string _sourceRoot;

        public SolutionDirectoryPackageLoader(string sourceRoot)
        {
            _sourceRoot = sourceRoot;
        }

        public IEnumerable<IPackageInfo> Load(IPackageLog log)
        {
            var manifestReader = new PackageManifestReader(new FileSystem(), folder => folder);
            
            //how can i 'where' the manifests
               

            var pis = PackageManifest.FindManifestFilesInDirectory(_sourceRoot)
                .Select(Path.GetDirectoryName)
                .Select(manifestReader.LoadFromFolder);

            var filtered = pis.Where(pi=>BottleRoles.Module.Equals(pi.Role));

            LogWriter.PrintHorizontalLine();
            LogWriter.Trace("Solution Package Loader found:");
            LogWriter.Indent(() =>
            {
                filtered.Each(p => LogWriter.Trace(p.Name));
            });

            LogWriter.PrintHorizontalLine();

            return filtered;
        }
    }
}