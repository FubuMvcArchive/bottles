using System.Collections.Generic;

namespace Bottles.PackageLoaders.Assemblies
{
    public interface IAssemblyFileFinder
    {

        AssemblyFiles FindAssemblies(string binDirectory, IEnumerable<string> assembliesToFind);
    }
}