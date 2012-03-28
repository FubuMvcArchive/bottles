using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FubuCore;

namespace Bottles.PackageLoaders.Assemblies
{
    /// <summary>
    /// Reperesents a collection of assembly files as found
    /// by an IAssemblyFileFinder
    /// </summary>
    [DebuggerDisplay("{debuggerDisplay()}")]
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

        //used implicitly
        string debuggerDisplay()
        {
            return "Files:{0} Pdb:{1} Success:{2}".ToFormat(Files.Count(), PdbFiles.Count(), Success);
        }
    }
}