using System;
using System.Collections.Generic;
using System.IO;
using Bottles.PackageLoaders.Assemblies;

namespace Bottles
{
    public interface IPackageInfo
    {
        string Name { get; }
        string Role { get; }
        void LoadAssemblies(IAssemblyRegistration loader);

        void ForFolder(string folderName, Action<string> onFound);
        void ForData(string searchPattern, Action<string, Stream> dataCallback);

        IEnumerable<Dependency> GetDependencies();
    }
}