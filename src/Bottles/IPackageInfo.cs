using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        PackageManifest Manifest { get; }
    }

    [DebuggerDisplay("{Name}:{Role}")]
    public class PackageInfo : IPackageInfo
    {
        private readonly PackageManifest _manifest;
        private readonly PackageFiles _files = new PackageFiles();
        private readonly IList<Dependency> _dependencies = new List<Dependency>();
        private readonly IList<AssemblyTarget> _assemblies = new List<AssemblyTarget>();

        public PackageInfo(PackageManifest manifest)
        {
            _manifest = manifest;
        }

        public string Name { get { return _manifest.Name; } }
        public string Role { get { return _manifest.Role; } }
        public string Description{ get; set; }

        public void LoadAssemblies(IAssemblyRegistration loader)
        {
            //double double dispatch - getting deep
            _assemblies.Each(a => a.Load(loader));
        }

        public PackageManifest Manifest
        {
            get { return _manifest; }
        }

        public  IEnumerable<Dependency> GetDependencies()
        {
            return _dependencies;
        }

        public void ForFolder(string folderName, Action<string> onFound)
        {
            _files.ForFolder(folderName, onFound);
        }

        protected void AddAssembly(AssemblyTarget assemblyTarget)
        {
            _assemblies.Add(assemblyTarget);
        }

        protected void AddAssembly(Assembly assembly)
        {
            _assemblies.Add(AssemblyTarget.FromAssembly(assembly));
        }

        public void ForData(string searchPattern, Action<string, Stream> dataCallback)
        {
            _files.ForData(searchPattern, dataCallback);
        }


        public void AddDependency(Dependency dependency)
        {
            _dependencies.Fill(dependency);
        }

        public Dependency[] Dependencies
        {
            get { return _dependencies.ToArray(); }
            set
            {
                _dependencies.Clear();
                _dependencies.AddRange(value);
            }
        }

        public PackageFiles Files
        {
            get { return _files; }
        }

        public void RegisterFolder(string folderName, string directory)
        {
            Files.RegisterFolder(folderName, directory);
        }

        public void RegisterAssemblyLocation(string assemblyName, string filePath)
        {
            AddAssembly(new AssemblyTarget()
            {
                AssemblyName = assemblyName,
                FilePath = filePath
            });
        }  

        public bool Equals(PackageInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name) && Equals(other.Description, Description);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(PackageInfo)) return false;
            return Equals((PackageInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Description != null ? Description.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return Description;
        }
        
        public class AssemblyTarget
        {
            public string AssemblyName { get; set; }
            public string FilePath { get; set; }

            public void Load(IAssemblyRegistration loader)
            {
                loader.LoadFromFile(FilePath, AssemblyName);
            }

            public static AssemblyTarget FromAssembly(Assembly assembly)
            {
                return new AssemblyTarget()
                           {
                               AssemblyName = assembly.FullName,
                               FilePath = assembly.Location
                           };
            }
        }
    }
}