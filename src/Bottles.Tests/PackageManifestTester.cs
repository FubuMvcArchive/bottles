using Bottles.Assemblies;
using Bottles.Commands;
using Bottles.Creation;
using Bottles.Zipping;
using FubuCore;
using NUnit.Framework;
using FubuTestingSupport;
using System.Linq;

namespace Bottles.Tests
{
    [TestFixture]
    public class PackageManifestTester
    {
        [Test]
        public void fileset_for_searching()
        {
            var fileSet = PackageManifest.FileSetForSearching();
            fileSet.DeepSearch.ShouldBeTrue();
            fileSet.Include.ShouldEqual(PackageManifest.FILE);
            fileSet.Exclude.ShouldBeNull();
        }

        [Test]
        public void set_role_to_module()
        {
            var manifest = new PackageManifest();
            manifest.SetRole(BottleRoles.Module);


            manifest.Role.ShouldEqual(BottleRoles.Module);

            manifest.ContentFileSet.ShouldNotBeNull();
            manifest.ContentFileSet.Include.ShouldContain("*.as*x");
        }

        [Test]
        public void set_role_to_config()
        {
            var manifest = new PackageManifest();
            manifest.SetRole(BottleRoles.Config);

            manifest.Role.ShouldEqual(BottleRoles.Config);

            manifest.ContentFileSet.ShouldBeNull();
            manifest.DataFileSet.ShouldBeNull();
            manifest.ConfigFileSet.ShouldNotBeNull();

            manifest.ConfigFileSet.Include.ShouldEqual("*.*");

            manifest.Assemblies.Any().ShouldBeFalse();
            
        }


        [Test]
        public void read_config_manifest_from_file()
        {
            var manifest = new PackageManifest();
            manifest.SetRole(BottleRoles.Config);

            var system = new FileSystem();
            system.WriteObjectToFile("manifest.xml", manifest);

            var manifest2 = system.LoadFromFile<PackageManifest>("manifest.xml");
            manifest2.ContentFileSet.ShouldBeNull();
        }

        [Test]
        public void set_role_to_binaries()
        {
            var manifest = new PackageManifest();
            manifest.SetRole(BottleRoles.Binaries);

            manifest.ContentFileSet.ShouldBeNull();
            manifest.DataFileSet.ShouldBeNull();
            manifest.ConfigFileSet.ShouldBeNull();
        }

        [Test]
        public void set_role_to_data()
        {
            var manifest = new PackageManifest();
            manifest.SetRole(BottleRoles.Data);

            manifest.ContentFileSet.ShouldBeNull();
            manifest.ConfigFileSet.ShouldBeNull();

            manifest.DataFileSet.DeepSearch.ShouldBeTrue();
            manifest.DataFileSet.Include.ShouldEqual("*.*");
        }

    }
}