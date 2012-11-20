using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Tests.IntegrationTesting
{
    [TestFixture]
    public class ZipPackageTesting : IntegrationTestDriver
    {
        private BottleLoadingDomain _domain;

        [SetUp]
        public void SetUp()
        {
            _domain = new BottleLoadingDomain();

            ResetBottleProjectCode();
        }

        [TearDown]
        public void TearDown()
        {
            _domain.Dispose();
        }

        [Test]
        public void read_data_and_web_content_from_a_zipped_package()
        {
            RunBottlesCommand("init bottles-staging BottlesProject");

            AlterManifest(manifest => {
                manifest.RemoveAllAssemblies();
                manifest.AddAssembly("BottleProject");
            });

            Recompile();
            
            RunBottlesCommand("create bottles-staging -o zips/BottleProject.zip");

            _domain.Proxy.LoadViaZip(ZipsDirectory);
            _domain.Proxy.ReadWebContent("content/scripts/script1.js").Trim()
                   .ShouldEqual("var x = 1;");

            _domain.Proxy.ReadData("1.txt").Trim()
                   .ShouldEqual("Original Value");
        }
    }
}