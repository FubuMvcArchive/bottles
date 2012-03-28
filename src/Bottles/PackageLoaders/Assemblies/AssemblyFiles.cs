using System.Collections.Generic;
using System.Linq;

namespace Bottles.PackageLoaders.Assemblies
{
    public class AssemblyFiles
    {
        public IEnumerable<string> Files { get; set; }
        public IEnumerable<string> PdbFiles { get; set; }

        public IEnumerable<string> MissingAssemblies { get; set; }

        public bool Success { get { return !MissingAssemblies.Any(); } }

        public static AssemblyFiles Empty
        {
            get
            {
                return new AssemblyFiles
                {
                    Files = new string[0],
                    PdbFiles = new string[0],
                    MissingAssemblies = new string[0]
                };
            }
        }
    }
}