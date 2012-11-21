using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Bottles.Manifest;
using FubuCore;
using FubuCore.Descriptions;

namespace Bottles
{
    [XmlType("package")]
    public class PackageManifest 
    {
        public const string FILE = ".package-manifest";

        public static FileSet FileSetForSearching()
        {
            return new FileSet(){
                DeepSearch = true,
                Include = FILE
            };
        }

        public static IEnumerable<string> FindManifestFilesInDirectory(string directory)
        {
            return new FileSystem().FindFiles(directory, FileSetForSearching());
        }

        public PackageManifest()
        {
            Role = BottleRoles.Module;
        }

        [XmlIgnore]
        public string ManifestFileName { get; set; }

        private readonly IList<string> _assemblies = new List<string>();

        


        public string Role { get; set; }
        public string Name { get; set; }
        public string BinPath { get; set; }

        [XmlElement("assembly")]
        public string[] Assemblies 
        {
            get
            {
                return _assemblies.ToArray();
            }
            set
            {
                _assemblies.Clear();

                if (value == null) return;
                _assemblies.AddRange(value);
            }
        }

        public bool AddAssembly(string assemblyName)
        {
            if (_assemblies.Contains(assemblyName))
            {
                return false;
            }

            _assemblies.Add(assemblyName);
            return true;
        }

        public FileSet ContentFileSet
        {
            get
            {
                return _contentFileSet;
            }
            set
            {
                _contentFileSet = value;

                if (_contentFileSet != null)
                {
                    _contentFileSet.AppendExclude(BottleFiles.DataFiles.Include);
                    _contentFileSet.AppendExclude(BottleFiles.ConfigFiles.Include);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("Package: {0}", Name);
        }

        public void RemoveAllAssemblies()
        {
            _assemblies.Clear();
        }

        public void RemoveAssembly(string assemblyName)
        {
            _assemblies.Remove(assemblyName);
        }

        public void SetRole(string role)
        {
            Role = role;



            switch (role)
            {
                case BottleRoles.Config:
                    ContentFileSet = null;
                    RemoveAllAssemblies();
                    break;

                case BottleRoles.Binaries:
                    ContentFileSet = null;
                    break;

                case BottleRoles.Data:
                    ContentFileSet = null;
                    RemoveAllAssemblies();
                    break;

                default:
                    ContentFileSet = new FileSet()
                    {
                        DeepSearch = true,
                        Include = "*.*",
                        Exclude = "*.cs;bin/*;obj/*;*.csproj*;packages.config;repositories.config;pak-*.zip;*.sln"
                    };
                    break;

            }
        }

        private readonly IList<Dependency> _dependencies = new List<Dependency>();
        private FileSet _contentFileSet;

        [XmlElement("dependency")]
        public Dependency[] Dependencies
        {
            get
            {
                return _dependencies.ToArray();
            }
            set
            {
                _dependencies.Clear();
                if (value != null) _dependencies.AddRange(value);
            }
        }

        public void AddDependency(string packageName, bool isMandatory)
        {
            _dependencies.RemoveAll(x => x.Name == packageName);
            _dependencies.Add(new Dependency(){
                IsMandatory = isMandatory,
                Name = packageName
            });
        }

        public void WriteTo(string directory)
        {
            new FileSystem().WriteObjectToFile(directory.AppendPath(FILE), this);
        }
    }
}