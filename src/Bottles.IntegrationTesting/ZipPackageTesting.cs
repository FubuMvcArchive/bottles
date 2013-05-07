using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.IntegrationTesting
{
    [TestFixture]
    public class ZipPackageTesting : IntegrationTestContext
    {
        [Test]
        public void read_data_and_web_content_from_a_zipped_package()
        {
            RunBottlesCommand("init bottles-staging BottleProject");

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

        [Test]
        public void config_bottle_should_have_proper_config_folder_in_zip()
        {
            CleanStagingDirectory();

            RunBottlesCommand("init bottles-staging BottleProject");

            AlterManifest(manifest =>
            {
                manifest.RemoveAllAssemblies();
                manifest.SetRole("config");
            });

            SetConfig("some config stuff");

            RunBottlesCommand("create bottles-staging -o zips/BottleProject.zip");

            _domain.Proxy.LoadViaZip(ZipsDirectory);
            
            _domain.Proxy
                .ReadConfig("1.txt").Trim()
                .ShouldEqual("some config stuff");
        }

        [Test]
        public void data_bottle_should_have_proper_data_folder_in_zip()
        {
            CleanStagingDirectory();

            RunBottlesCommand("init bottles-staging BottleProject");

            AlterManifest(manifest =>
            {
                manifest.RemoveAllAssemblies();
                manifest.SetRole("data");
            });

            SetData("some data stuff");

            RunBottlesCommand("create bottles-staging -o zips/BottleProject.zip");

            _domain.Proxy.LoadViaZip(ZipsDirectory);

            _domain.Proxy
                .ReadData("1.txt").Trim()
                .ShouldEqual("some data stuff");
        }

        [Test]
        public void bottle_should_be_reexploded_when_the_versioning_changes()
        {
            RunBottlesCommand("init bottles-staging BottleProject");

            AlterManifest(manifest =>
            {
                manifest.RemoveAllAssemblies();
                manifest.AddAssembly("BottleProject");
            });

            Recompile();

            RunBottlesCommand("create bottles-staging -o zips/BottleProject.zip");

            // Check the initial state
            _domain.Proxy.LoadViaZip(ZipsDirectory);
            _domain.Proxy.ReadWebContent("content/scripts/script1.js").Trim()
                   .ShouldEqual("var x = 1;");

            // Now mess with things
            SetData("Different Value");
            SetContent("var z = 2;");

            // And rebuild the zip
            Recompile();
            RunBottlesCommand("create bottles-staging -o zips/BottleProject.zip -f");

            _domain.Recycle();

            _domain.Proxy.LoadViaZip(ZipsDirectory);
            _domain.Proxy.ReadWebContent("content/scripts/script1.js").Trim()
                   .ShouldEqual("var z = 2;");

            _domain.Proxy.ReadData("1.txt").Trim()
                   .ShouldEqual("Different Value");
        }
    }
}