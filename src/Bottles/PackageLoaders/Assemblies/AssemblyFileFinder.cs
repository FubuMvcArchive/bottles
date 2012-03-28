using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;

namespace Bottles.PackageLoaders.Assemblies
{

    public class AssemblyFileFinder : IAssemblyFileFinder
    {
        private readonly IFileSystem _fileSystem;

        public AssemblyFileFinder(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public AssemblyFiles FindAssemblies(string binDirectory, IEnumerable<string> assembliesToFind)
        {
            if (!assembliesToFind.Any()) return AssemblyFiles.Empty;

            var assemblySet = FileSet.ForAssemblyNames(assembliesToFind);
            var debugSet = FileSet.ForAssemblyDebugFiles(assembliesToFind);

            var files = new AssemblyFiles(){
                Files = _fileSystem.FindFiles(binDirectory, assemblySet),
                PdbFiles = _fileSystem.FindFiles(binDirectory, debugSet)
            };

            var assembliesFound = files.Files.Select(Path.GetFileNameWithoutExtension).Select(x => x.ToLowerInvariant());
            
            files.MissingAssemblies = assembliesToFind
                .Select(x => x.ToLowerInvariant())
                .Where(name => !assembliesFound.Contains(name));

            return files;
        }
    }
}