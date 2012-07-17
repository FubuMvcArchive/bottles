using System.Linq;
using Bottles.Diagnostics;
using Bottles.PackageLoaders.Directory;
using FubuCore;
using NUnit.Framework;
using FubuTestingSupport;

namespace Bottles.Tests
{
    [TestFixture]
    public class SolutionDirectoryBottleLoaderTester
    {
        private string thePathToScan = "solDirPackLoad";
        private DirectoryBottleLoader theLoader;

        [SetUp]
        public void BeforeEach()
        {
            var fs = new FileSystem();
            fs.DeleteDirectory(thePathToScan);
            fs.CreateDirectory(thePathToScan);
            fs.CreateDirectory(thePathToScan, "bin");

            theLoader = new DirectoryBottleLoader(thePathToScan.ToFullPath());
            var manifest = new PackageManifest{
                Name = "test-mani"
            };

            fs.PersistToFile(manifest, thePathToScan, PackageManifest.FILE);
        }

        [Test]
        public void there_are_7_manifests_that_are_modules_in_fubu()
        {
            var foundPackages = theLoader.Load(new BottleLog());
            foundPackages.Count().ShouldEqual(1);
            foundPackages.First().Name.ShouldEqual("test-mani");
        }
    }
}