using System.IO;
using System.Linq;
using System.Reflection;
using AssemblyPackage;
using AttributeMarkedBottle;
using Bottles.PackageLoaders.Assemblies;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;
using FubuCore;

namespace Bottles.Tests
{
    [TestFixture]
    public class AssemblyBottleInfoTester
    {
        private Assembly assembly;
        private IPackageInfo package;

        [SetUp]
        public void SetUp()
        {
            assembly = Assembly.GetExecutingAssembly();
            package = new AssemblyPackageInfo(assembly);
        }

        [Test]
        public void name_just_returns_the_assembly_name()
        {
            package.Name.ShouldEqual(assembly.GetName().Name);
        }

        [Test]
        public void load_assemblies_just_tries_to_add_the_inner_assembly_directly()
        {
            var loader = MockRepository.GenerateMock<IAssemblyRegistration>();
            package.LoadAssemblies(loader);

            loader.AssertWasCalled(x => x.Use(assembly));
        }

        [Test]
        public void get_dependencies_is_empty_FOR_NOW()
        {
            package.Dependencies.Any().ShouldBeFalse();
        }
    }

    [TestFixture]
    public class AssemblyPackageInfoIntegratedTester
    {
        private IPackageInfo thePackage;

        [SetUp]
        public void SetUp()
        {
            new FileSystem().DeleteDirectory("content");
            thePackage = new AssemblyPackageInfo(typeof (AssemblyPackageMarker).Assembly);
        }

        [Test]
        public void can_read_the_package_manifest_from_the_assembly_when_it_is_an_embedded_resource()
        {
            var assembly = typeof (AssemblyPackageMarker).Assembly;
            var manifest = new AssemblyPackageManifestFactory().Extract(assembly);

            manifest.Name.ShouldEqual("FakeProject");
            manifest.Role.ShouldEqual("module");
        }

        [Test]
        public void can_read_the_package_manifest_from_a_bottle_attribute()
        {
            var assembly = typeof (AttributeMarkedBottleMarker).Assembly;
            var manifest = new AssemblyPackageManifestFactory().Extract(assembly);

            manifest.Name.ShouldEqual("SpecialBottle");
            manifest.Dependencies.Select(x => x.Name).ShouldHaveTheSameElementsAs("foo1", "foo2");
        }

        [Test]
        public void can_retrieve_data_from_package()
        {
            var text = "not the right thing";
            thePackage.ForFiles(BottleFiles.DataFolder, "1.txt", (name, data) =>
            {
                name.ShouldEqual("data{0}1.txt".ToFormat(Path.DirectorySeparatorChar));
                text = new StreamReader(data).ReadToEnd();
            });

            // The text of this file in the AssemblyPackage data is just "1"
            text.ShouldEqual("1");
        }

        [Test]
        public void can_retrieve_web_content_folder_from_package()
        {
            var expected = "not this";
            thePackage.ForFolder(BottleFiles.WebContentFolder, folder =>
            {
                expected = folder;
            });

            expected.ShouldEqual("content".AppendPath("AssemblyPackage", "WebContent").ToFullPath());
        }
    }
}