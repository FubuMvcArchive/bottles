using System;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;

namespace Bottles.Tests
{
    [TestFixture]
    public class BottleOrderingIntegratedTester
    {
        private void loadPackages(Action<StubPackageLoader> configuration)
        {
            PackageRegistry.LoadPackages(facility =>
            {
                var loader = new StubPackageLoader();
                configuration(loader);

                facility.Loader(loader);
            });
        }

        private void thePackageNamesInOrderShouldBe(params string[] names)
        {
            PackageRegistry.Packages.Select(x => x.Name)
                .ShouldHaveTheSameElementsAs(names);
        }

        [Test]
        public void orders_by_name()
        {
            loadPackages(x =>
            {
                x.HasPackage("D");
                x.HasPackage("C");
                x.HasPackage("B");
            });

            thePackageNamesInOrderShouldBe("B", "C", "D");
        }

        [Test]
        public void order_with_a_dependency()
        {
            loadPackages(x =>
            {
                x.HasPackage("A1");
                x.PackageFor("A").MandatoryDependency("A1");
            });

            PackageRegistry.AssertNoFailures();

            thePackageNamesInOrderShouldBe("A1", "A");
        }

        [Test]
        public void logs_failure_with_missing_dependency()
        {
            loadPackages(x =>
            {
                x.PackageFor("A").MandatoryDependency("B");
            });

            Exception<ApplicationException>.ShouldBeThrownBy(() =>
            {
                PackageRegistry.AssertNoFailures();
            }).Message.ShouldContain("Missing required Bottle/Package dependency named 'B'");
        }
    }
}