using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using FubuCore;

namespace Bottles
{
    [XmlType("package")]
    public class PackageManifest
    {
        public static readonly string FILE = ".package-manifest";

        public PackageManifest()
        {
            Role = BottleRoles.Module;

            DataFileSet = new FileSet();
            ContentFileSet = new FileSet(){
                Include = "*.as*x;*.master;Content{0}*.*;*.config".ToFormat(Path.DirectorySeparatorChar),
                Exclude = "data/*"
            };
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

        public FileSet DataFileSet
        {
            get; set;
        }

        public FileSet ContentFileSet
        {
            get; set;
        }

        public FileSet ConfigFileSet
        {
            get; set;
        }



        /// <summary>
        /// The class to run during an install
        /// </summary>
        public string EnvironmentClassName { get; set; }

        /// <summary>
        /// The assembly where the environment class is located
        /// </summary>
        public string EnvironmentAssembly { get; set; }

        /// <summary>
        /// The configuration file to use during install
        /// </summary>
        public string ConfigurationFile { get; set; }



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
                    ConfigFileSet = new FileSet(){Include = "*.*", DeepSearch = true};
                    ContentFileSet = null;
                    DataFileSet = null;
                    break;

                case BottleRoles.Binaries:
                    ConfigFileSet = null;
                    ContentFileSet = null;
                    DataFileSet = null;
                    break;

            }
        }
    }
}