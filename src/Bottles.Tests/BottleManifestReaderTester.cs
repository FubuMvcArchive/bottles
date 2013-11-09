using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Bottles.Manifest;
using Bottles.PackageLoaders.Assemblies;
using FubuCore;
using NUnit.Framework;
using FubuTestingSupport;

namespace Bottles.Tests
{
    [TestFixture]
    public class when_reading_a_package_from_a_folder
    {
        private PackageManifest theOriginalManifest;
        private IPackageInfo thePackage;

        [SetUp]
        public void SetUp()
        {
            var system = new FileSystem();
            system.DeleteDirectory("package1");

            system.CreateDirectory("package1");
            system.CreateDirectory("package1", "bin");
            system.CreateDirectory("package1", "WebContent");
            system.CreateDirectory("package1", "Data");

            theOriginalManifest = new PackageManifest
            {
                Assemblies = new[] { "a", "b", "c" },
                Name = "Extraordinary"
            };

            theOriginalManifest.AddDependency("bottle1", true);
            theOriginalManifest.AddDependency("bottle2", true);
            theOriginalManifest.AddDependency("bottle3", false);

            theOriginalManifest.WriteTo("package1");

            thePackage = new PackageManifestReader(new FileSystem(), directory => directory.AppendPath("WebContent")).LoadFromFolder("package1");
        }

        [Test]
        public void has_all_the_dependencies()
        {
            thePackage.Dependencies
                .ShouldHaveTheSameElementsAs(Dependency.Mandatory("bottle1"), Dependency.Mandatory("bottle2"), Dependency.Optional("bottle3"));
        }

        [Test]
        public void reads_the_package_name()
        {
            thePackage.Name.ShouldEqual(theOriginalManifest.Name);
        }

    }

    [TestFixture]
    public class when_reading_a_package_from_a_folder_with_a_native_assembly
    {
        private PackageManifest theOriginalManifest;
        private IPackageInfo thePackage;

        [SetUp]
        public void SetUp()
        {
            var system = new FileSystem();
            system.DeleteDirectory("package1");

            system.CreateDirectory("package1");
            system.CreateDirectory("package1", "bin");
            system.WriteStringToFile(Path.Combine("package1", "bin", "a.dll"), "I'm a managed assembly");
            system.WriteStringToFile(Path.Combine("package1", "bin", "b.dll"), "I'm a native assembly");
            system.CreateDirectory("package1", "WebContent");
            system.CreateDirectory("package1", "Data");

            theOriginalManifest = new PackageManifest
            {
                Assemblies = new[] { "a" },
                NativeAssemblies = new[] { "b" },
                Name = "Extraordinary"
            };

            theOriginalManifest.WriteTo("package1");

            thePackage = new PackageManifestReader(new FileSystem(), directory => directory.AppendPath("WebContent")).LoadFromFolder("package1");
        }

        [Test]
        public void should_have_loaded_not_native_assemblies_only()
        {
            var fakeAssemblyRegistration = new FakeAssemblyRegistration();
            thePackage.LoadAssemblies(fakeAssemblyRegistration);

            fakeAssemblyRegistration.AssembliesRequestedToBeLoaded.ShouldContain("a");
            fakeAssemblyRegistration.AssembliesRequestedToBeLoaded.ShouldNotContain("b");
        }
    }

    public class FakeAssemblyRegistration : IAssemblyRegistration
    {
        public FakeAssemblyRegistration()
        {
            AssembliesRequestedToBeLoaded = new List<string>();
        }

        public List<string> AssembliesRequestedToBeLoaded { get; set; }

        public void Use(Assembly assembly)
        {
            throw new System.NotImplementedException();
        }

        public void LoadFromFile(string fileName, string assemblyName)
        {
            AssembliesRequestedToBeLoaded.Add(assemblyName);
        }
    }
}