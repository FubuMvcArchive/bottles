using NUnit.Framework;
using FubuTestingSupport;
using System.Linq;

namespace Bottles.Tests
{
    [TestFixture]
    public class PackageManifestTester
    {
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
        public void set_role_to_binaries()
        {
            var manifest = new PackageManifest();
            manifest.SetRole(BottleRoles.Binaries);

            manifest.ContentFileSet.ShouldBeNull();
            manifest.DataFileSet.ShouldBeNull();
            manifest.ConfigFileSet.ShouldBeNull();
        }
    }
}