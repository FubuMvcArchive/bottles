using Bottles.Tests.Harness;
using NUnit.Framework;

namespace Bottles.Tests.Services.Integration
{
    [TestFixture, Explicit("These tests all require manual intervention to run")]
    [Platform(Exclude = "Unix,Linux")]
    public class service_activation_integration_tester
    {
        /* NOTE:  You have to manually type CTRL-C into the window that pops up to complete
         * these tests.  It'll be obvious what to do when you run it;)
         * 
         * If the tests whine about BottleServiceRunner.exe not being found,
         * you might just Rebuild the solution
         */

        [Test]
        public void loading_a_single_service_bottle_with_defaults_and_BottleService_attribute()
        {
            new ServiceRunner("BottleService1", "1").Execute();
        }

        [Test]
        public void loading_a_single_service_bottle_with_defaults_and_BottleService_attribute_and_multiple_activators()
        {
            new ServiceRunner("BottleService2", "2a", "2b").Execute();
        }

        [Test]
        public void loading_multiple_bottle_services()
        {
            new ServiceRunner("BottleService3", "1", "2a", "2b", "3")
                .Execute();
        }

        [Test]
        public void loading_service_with_an_application_loader()
        {
            new ServiceRunner("ApplicationLoaderService", "loader")
                .Execute();
        }

        [Test]
        public void loading_service_with_an_application_source()
        {
            new ServiceRunner("ApplicationSourceService", "source")
                .Execute();
        }
    }
}