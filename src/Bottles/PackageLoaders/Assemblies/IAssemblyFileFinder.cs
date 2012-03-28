using System.Collections.Generic;

namespace Bottles.PackageLoaders.Assemblies
{
    /// <summary>
    /// Finds all of the assemblies in a given directory.
    /// </summary>
    public interface IAssemblyFileFinder
    {

        AssemblyFiles FindAssemblies(string binDirectory, IEnumerable<string> assembliesToFind);
    }
}