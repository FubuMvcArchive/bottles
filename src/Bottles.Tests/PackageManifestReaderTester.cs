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

            theOriginalManifest = new PackageManifest(){
                Assemblies = new string[]{"a", "b", "c"},
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
            thePackage.GetDependencies()
                .ShouldHaveTheSameElementsAs(Dependency.Mandatory("bottle1"), Dependency.Mandatory("bottle2"), Dependency.Optional("bottle3"));
        }

        [Test]
        public void reads_the_package_name()
        {
            thePackage.Name.ShouldEqual(theOriginalManifest.Name);
        }

    }
}