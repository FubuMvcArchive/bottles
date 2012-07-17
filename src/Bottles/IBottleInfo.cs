using System;
using System.IO;
using Bottles.PackageLoaders.Assemblies;

namespace Bottles
{
    public interface IBottleInfo
    {
        string Name { get; }
        string Role { get; }
        string Description { get; }

        void LoadAssemblies(IAssemblyRegistration loader);
        void ForFolder(string folderName, Action<string> onFound);
        void ForFiles(string directory, string searchPattern, Action<string, Stream> fileCallback);

        Dependency[] Dependencies { get; }
        PackageManifest Manifest { get; }
        IBottleFiles Files { get; }
    }
}