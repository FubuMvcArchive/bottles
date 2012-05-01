using System.Diagnostics;
using System.Reflection;

namespace Bottles.PackageLoaders.Assemblies
{
    /// <summary>
    /// Reperesents a bottle that is contained in a .dll
    /// </summary>
    [DebuggerDisplay("{Name}:{Role}")]
    public class AssemblyPackageInfo : PackageInfo
    {
        private readonly Assembly _assembly;

        public AssemblyPackageInfo(Assembly assembly) :
            base(new AssemblyPackageManifestFactory().Extract(assembly))
        {
            _assembly = assembly;
            AddAssembly(_assembly);
        }
    }
}