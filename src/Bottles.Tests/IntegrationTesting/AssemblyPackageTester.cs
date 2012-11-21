using FubuTestingSupport;
using NUnit.Framework;
using System.Linq;

namespace Bottles.Tests.IntegrationTesting
{
    [TestFixture]
    public class AssemblyPackageTester : IntegrationTestContext
    {
        [Test]
        public void read_contents_without_any_explicit_manifest()
        {
            RunBottlesCommand("assembly-pak bottles-staging");
            Recompile();

            _domain.Proxy.LoadViaAssembly();

            _domain.Proxy.ReadWebContent("content/scripts/script1.js").Trim()
                   .ShouldEqual("var x = 1;");

            _domain.Proxy.ReadData("1.txt").Trim()
                   .ShouldEqual("Original Value");

        }

        [Test]
        public void read_contents_with_explicit_manifest()
        {
            RunBottlesCommand("assembly-pak bottles-staging --init");

            AlterManifest(manifest => {
                manifest.AddDependency("FubuCore", false);
            });

            RunBottlesCommand("assembly-pak bottles-staging");
            Recompile();

            _domain.Proxy.LoadViaAssembly();

            _domain.Proxy.ReadWebContent("content/scripts/script1.js").Trim()
                   .ShouldEqual("var x = 1;");

            _domain.Proxy.ReadData("1.txt").Trim()
                   .ShouldEqual("Original Value");

            _domain.Proxy.BottleDependencies().Single().Name.ShouldEqual("FubuCore");
        }

        [Test]
        public void re_explode_based_on_a_newer_assembly_version()
        {
            RunBottlesCommand("assembly-pak bottles-staging");
            SetAssemblyVersion("1.0.0.0");
            
            Recompile();


            _domain.Proxy.LoadViaAssembly();

            _domain.Proxy.ReadWebContent("content/scripts/script1.js").Trim()
                   .ShouldEqual("var x = 1;");

            _domain.Proxy.ReadData("1.txt").Trim()
                   .ShouldEqual("Original Value");

            _domain.Recycle();


            // Now, let's get a new version of the assembly
            SetAssemblyVersion("1.0.0.1");

            // Now mess with things
            SetData("Different Value");
            SetContent("var z = 2;");
            RunBottlesCommand("assembly-pak bottles-staging");

            // And rebuild the zip
            Recompile();

            _domain.Proxy.LoadViaAssembly();

            // Check the new values
            _domain.Proxy.ReadWebContent("content/scripts/script1.js").Trim()
                   .ShouldEqual("var z = 2;");

            _domain.Proxy.ReadData("1.txt").Trim()
                   .ShouldEqual("Different Value");
        }
    }
}