using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bottles.Commands;
using Bottles.Creation;
using Bottles.PackageLoaders.Assemblies;
using FubuCore;

namespace Bottles.Diagnostics
{
    public class BottleLogger : IBottleLogger
    {
        private readonly LoggingSession _log;

        public BottleLogger(LoggingSession log)
        {
            _log = log;
        }

        public void WriteAssembliesNotFound(AssemblyFiles theAssemblyFiles, PackageManifest manifest, CreateBottleInput input, string binFolder)
        {
            var log = _log.LogFor(manifest);

            var sb = new StringBuilder();
            sb.AppendFormat("Did not locate all designated assemblies at '{0}'", binFolder.ToFullPath());
            sb.AppendLine();


            sb.AppendLine("Looking for these assemblies in the package manifest file:");
            manifest.Assemblies.Each(name => sb.AppendLine("  " + name));


            sb.AppendLine("But only found:");
            if(!theAssemblyFiles.Files.Any()) sb.AppendLine("  Found no files");
            theAssemblyFiles.Files.Each(file => sb.AppendLine("  " + file));

            sb.AppendLine("Missing");
            theAssemblyFiles.MissingAssemblies.Each(file => sb.AppendLine("  " + file));

            log.MarkFailure(sb.ToString());
        }
    }
}