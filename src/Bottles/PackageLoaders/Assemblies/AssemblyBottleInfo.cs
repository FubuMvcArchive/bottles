using System.Diagnostics;
using System.Reflection;

namespace Bottles.PackageLoaders.Assemblies
{
    /// <summary>
    /// Reperesents a bottle that is contained in a .dll
    /// </summary>
    [DebuggerDisplay("{Name}:{Role}")]
    public class AssemblyBottleInfo : BottleInfo
    {
        private readonly Assembly _assembly;

        public AssemblyBottleInfo(Assembly assembly) :
            base(new AssemblyBottleManifestFactory().Extract(assembly))
        {
            _assembly = assembly;
            AddAssembly(_assembly);
        }
    }
}