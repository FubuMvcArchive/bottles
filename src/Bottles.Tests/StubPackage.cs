using System;
using System.Collections.Generic;
using System.IO;
using Bottles.Assemblies;
using FubuCore.Util;

namespace Bottles.Tests
{
    public class StubPackage : IPackageInfo
    {
        private readonly string _name;
        private readonly Cache<string, string> _folderNames = new Cache<string,string>();

        public StubPackage(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Role { get; set; }

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
        public IList<Dependency> Dependencies
        {
            get
            {
                return _dependencies;
            }
        }

        public IEnumerable<Dependency> GetDependencies()
        {
            return _dependencies;
        }

        public Action<IAssemblyRegistration> LoadingAssemblies { get; set; }
    }
}