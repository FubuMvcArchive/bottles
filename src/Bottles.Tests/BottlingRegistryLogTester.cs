using Bottles.Diagnostics;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Tests
{
    [TestFixture]
    public class BottlingRegistryLogTester
    {
        [Test]
        public void write_trace()
        {
            var log = new PackageLog();
            log.FullTraceText().ShouldBeEmpty();

            log.Trace("stuff");
            log.Trace("other");
            log.Trace("new");

            log.FullTraceText().ShouldContain("stuff");
            log.FullTraceText().ShouldContain("other");
            log.FullTraceText().ShouldContain("new");
        }

        [Test]
        public void find_children()
        {
            var log = new PackageLog();

            var loader1 = new StubBottleLoader();
            var loader2 = new StubBottleLoader();
            var loader3 = new StubBottleLoader();

            var package1 = new StubBottle("1");
            var package2 = new StubBottle("2");
            var package3 = new StubBottle("3");
        
            log.AddChild(loader1, loader2, loader3, package1, package2, package3);

            log.FindChildren<IBottleLoader>().ShouldHaveTheSameElementsAs(loader1, loader2, loader3);

            log.FindChildren<IPackageInfo>().ShouldHaveTheSameElementsAs(package1, package2, package3);
        }
    }
}