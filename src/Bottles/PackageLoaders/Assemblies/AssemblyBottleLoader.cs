using System.Collections.Generic;
using System.Reflection;
using Bottles.Diagnostics;

namespace Bottles.PackageLoaders.Assemblies
{
    /// <summary>
    /// Loads a package from an assembly (.dll / .exe)
    /// </summary>
    public class AssemblyBottleLoader : IBottleLoader
    {
        private readonly Assembly _assembly;

        public AssemblyBottleLoader(Assembly assembly)
        {
            _assembly = assembly;
        }

        public IEnumerable<IBottleInfo> Load(IBottleLog log)
        {
            yield return AssemblyBottleInfoFactory.CreateFor(_assembly);
        }

        public override string ToString()
        {
            return string.Format("Assembly: {0}", _assembly);
        }

        public bool Equals(AssemblyBottleLoader other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._assembly, _assembly);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (AssemblyBottleLoader)) return false;
            return Equals((AssemblyBottleLoader) obj);
        }

        public override int GetHashCode()
        {
            return (_assembly != null ? _assembly.GetHashCode() : 0);
        }
    }
}