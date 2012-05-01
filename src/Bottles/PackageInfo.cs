using System.Diagnostics;

namespace Bottles
{

    /// <summary>
    /// This is a normal/traditional bottle that is represented as a folder/zipfile
    /// </summary>
    [DebuggerDisplay("{Name}:{Role}")]
    public class PackageInfo : BasePackageInfo
    {
        public PackageInfo(PackageManifest manifest) : base(manifest)
        {
        }

        public void RegisterFolder(string folderName, string directory)
        {
            Files.RegisterFolder(folderName, directory);
        }

        public void RegisterAssemblyLocation(string assemblyName, string filePath)
        {
            AddAssembly(new AssemblyTarget(){
                AssemblyName = assemblyName, 
                FilePath = filePath
            });
        }       
    }
}