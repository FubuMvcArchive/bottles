﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Bottles.Exploding;
using Bottles.Manifest;
using FubuCore;
using FubuCore.Descriptions;

namespace Bottles.PackageLoaders.Assemblies
{
    /// <summary>
    /// Reperesents a bottle that is contained in a .dll
    /// </summary>
    [DebuggerDisplay("{Name}:{Role}")]
    public class AssemblyPackageInfo : IPackageInfo, DescribesItself
    {
        public static AssemblyPackageInfo For(string fileName)
        {
            var assembly = AssemblyLoader.LoadPackageAssemblyFromAppBinPath(fileName);
            return new AssemblyPackageInfo(assembly);
        }

        private readonly Assembly _assembly;
        private readonly Lazy<PackageInfo> _inner;
        private readonly PackageManifest _manifest;

        public AssemblyPackageInfo(Assembly assembly)
        {
            _manifest = DetermineManifest(assembly);
            _inner = new Lazy<PackageInfo>(() =>
            {
                var inner = new PackageInfo(_manifest);

                var exploder = BottleExploder.GetPackageExploder(new FileSystem());
                exploder.ExplodeAssembly(PackageRegistry.GetApplicationDirectory(), assembly, inner);

                return inner;
            });

            _assembly = assembly;
        }

        public string Name
        {
            get { return _manifest.Name; }
        }

        public string Role
        {
            get { return _inner.Value.Role; }
        }

        public string Description
        {
            get { return "Assembly: " + _assembly.GetName().Name; }
        }

        public void LoadAssemblies(IAssemblyRegistration loader)
        {
            loader.Use(_assembly);
        }

        public void ForFolder(string folderName, Action<string> onFound)
        {
            _inner.Value.ForFolder(folderName, onFound);
        }

        public void ForFiles(string directory, string searchPattern, Action<string, Stream> fileCallback)
        {
            _inner.Value.ForFiles(directory, searchPattern, fileCallback);
        }

        public Dependency[] Dependencies
        {
            get { return _manifest.Dependencies; }
        }

        public PackageManifest Manifest
        {
            get { return _manifest; }
        }

        public IPackageFiles Files
        {
            get { return _inner.Value.Files; }
        }

        public void Describe(Description description)
        {
            description.ShortDescription = "Assembly:  " + _assembly.GetName().Name;
            description.Properties["Assembly"] = _assembly.GetName().FullName;

            description.AddChild("Inner", _inner.Value);
        }

        public static PackageManifest DetermineManifest(Assembly assembly)
        {
            var resourceName = assembly.GetName().Name + "." + PackageManifest.FILE;

            using (var resource = assembly.GetManifestResourceStream(resourceName))
            {
                if (resource != null)
                {
                    var manifest = new PackageManifestReader(new FileSystem(), s => s).LoadFromStream(resource);
                    if (manifest.Name.IsEmpty())
                    {
                        manifest.Name = assembly.GetName().Name;
                    }

                    return manifest;
                }
            }

            return defaults(assembly);
        }


        private static PackageManifest defaults(Assembly assembly)
        {
            return new PackageManifest
                   {
                       Name = assembly.GetName().Name,
                       Role = BottleRoles.Module,
                       Assemblies = new[] { assembly.FullName },
                       Dependencies = new Dependency[0]
                   };
        }
    }
}