using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.PackageLoaders.Assemblies;
using FubuCore.Util;

namespace Bottles.Tests
{
    public class StubPackage : IPackageInfo
    {
        private readonly Cache<string, string> _folderNames = new Cache<string,string>();
        private readonly PackageManifest _manifest;

        public StubPackage(string name) : this(new PackageManifest(){Name=name})
        {
            
        }
        public StubPackage(PackageManifest manifest)
        {
            _manifest = manifest;

            LoadingAssemblies = r => { };
        }

        public string Name
        {
            get { return _manifest.Name; }
        }

        public string Role { get { return _manifest.Role; } }

        public void LoadAssemblies(IAssemblyRegistration loader)
        {
            LoadingAssemblies(loader);
        }

        public void RegisterFolder(string folderAlias, string folderName)
        {
            _folderNames[folderAlias] = folderName;
        }

        public void ForFolder(string folderName, Action<string> onFound)
        {
            _folderNames.WithValue(folderName, onFound);
        }

        public void ForData(string searchPattern, Action<string, Stream> dataCallback)
        {
            throw new NotImplementedException();
        }

        public void OptionalDependency(string name)
        {
            _dependencies.Fill(new Dependency(){Name = name});
        }

        public void MandatoryDependency(string name)
        {
            _dependencies.Fill(new Dependency() { Name = name, IsMandatory = true});
        }

        private readonly IList<Dependency> _dependencies = new List<Dependency>();
        public Dependency[] Dependencies
        {
            get
            {
                return _dependencies.ToArray();
            }
        }


        public Action<IAssemblyRegistration> LoadingAssemblies { get; set; }

        public PackageManifest Manifest
        {
            get { return _manifest; }
        }
    }
}