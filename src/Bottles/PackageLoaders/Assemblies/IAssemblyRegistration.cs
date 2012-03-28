using System.Reflection;

namespace Bottles.PackageLoaders.Assemblies
{
    public interface IAssemblyRegistration
    {
        void Use(Assembly assembly);
        void LoadFromFile(string fileName, string assemblyName);
    }
}