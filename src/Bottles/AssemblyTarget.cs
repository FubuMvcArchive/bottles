using System.Reflection;
using Bottles.PackageLoaders.Assemblies;

namespace Bottles
{
    public class AssemblyTarget
    {
        public string AssemblyName { get; set; }
        public string FilePath { get; set; }

        public void Load(IAssemblyRegistration loader)
        {
            if (Assembly == null)
            {
                loader.LoadFromFile(FilePath, AssemblyName);
            }
            else
            {
                loader.Use(Assembly);
            }
        }

        public static AssemblyTarget FromAssembly(Assembly assembly)
        {
            return new AssemblyTarget()
                   {
                       Assembly = assembly
                   };
        }

        public Assembly Assembly { get; set; }
    }
}