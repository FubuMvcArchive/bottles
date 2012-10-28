using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Bottles.PackageLoaders.Assemblies;
using FubuCore.Descriptions;
using FubuCore;

namespace Bottles
{
    [DebuggerDisplay("{Name}:{Role}")]
    public class PackageInfo : IPackageInfo, DescribesItself
    {
        private readonly PackageFiles _files = new PackageFiles();
        private readonly IList<Dependency> _dependencies = new List<Dependency>();
        private readonly IList<AssemblyTarget> _assemblies = new List<AssemblyTarget>();

        public PackageInfo(PackageManifest manifest)
        {
            Manifest = manifest;
        }

        public PackageManifest Manifest { get; private set; }
        public string Name { get { return Manifest.Name; } }
        public string Role { get { return Manifest.Role; } }
        public string Description { get; set; }

        public void LoadAssemblies(IAssemblyRegistration loader)
        {
            //double double dispatch - getting deep
            _assemblies.Each(a => a.Load(loader));
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

        public void ForFolder(string folderName, Action<string> onFound)
        {
            _files.ForFolder(folderName, onFound);
        }

        public void ForFiles(string directory, string searchPattern, Action<string, Stream> fileCallback)
        {
            _files.GetFiles(directory, searchPattern, fileCallback);
        }

        protected void AddAssembly(AssemblyTarget assemblyTarget)
        {
            _assemblies.Add(assemblyTarget);
        }

        protected void AddAssembly(Assembly assembly)
        {
            _assemblies.Add(AssemblyTarget.FromAssembly(assembly));
        }

        public void AddDependency(Dependency dependency)
        {
            _dependencies.Fill(dependency);
        }

       

        public IPackageFiles Files
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

        public void Describe(Description description)
        {
            description.Title = "Package '{0}'".ToFormat(Name);
            description.Properties["Role"] = Role;

            description.Properties["Assemblies"] = Manifest.Assemblies.Join(", ");

            if (Manifest.BinPath != null) description.Properties["BinPath"] = Manifest.BinPath;

            if (Manifest.ContentFileSet != null) description.Properties["Content Files"] = Manifest.ContentFileSet.ToString();
            if (Manifest.DataFileSet != null) description.Properties["Data Files"] = Manifest.DataFileSet.ToString();
            if (Manifest.ConfigFileSet != null) description.Properties["Config Files"] = Manifest.ConfigFileSet.ToString();

            if (Dependencies != null && Dependencies.Any()) description.AddList("Dependencies", Dependencies);
        }

        public override string ToString()
        {
            return Description;
        }
    }
}