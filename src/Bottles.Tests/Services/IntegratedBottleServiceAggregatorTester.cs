using Bottles.Diagnostics;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Services.Tests
{
    [TestFixture]
    public class IntegratedBottleServiceAggregatorTester
    {
        private BottleServiceAggregator theAggregator;
        private BottleServiceRunner theRunner;
        private PackageLog theLog;
        private StubService theService;

        [SetUp]
        public void SetUp()
        {
            PackageRegistry.LoadPackages(x => x.Assembly(GetType().Assembly));
            theAggregator = new BottleServiceAggregator();

            theAggregator.Bootstrap(theLog);
            theRunner = theAggregator.ServiceRunner();

            theService = new StubService();
        }

        [Test]
        public void builds_the_aggregate_service_runner()
        {
            theRunner.Services.ShouldHaveTheSameElementsAs(new BottleService(theService, theLog));
        }
    }
}