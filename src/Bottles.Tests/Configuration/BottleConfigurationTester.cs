using Bottles.Configuration;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Tests.Configuration
{
    [TestFixture]
    public class BottleConfigurationTester
    {
        private BottleConfiguration theConfiguration;

        [SetUp]
        public void SetUp()
        {
            theConfiguration = new BottleConfiguration("Test");
        }

        [Test]
        public void registers_an_error()
        {
            var error = new FakeError();
            theConfiguration.RegisterError(error);

            theConfiguration.Errors.ShouldHaveTheSameElementsAs(error);
        }

        [Test]
        public void finds_the_missing_services()
        {
            var plugin = new MissingService(typeof(object));
            theConfiguration.RegisterError(plugin);

            theConfiguration.MissingServices.ShouldHaveTheSameElementsAs(plugin);
        }

        [Test]
        public void finds_the_missing_services_helper()
        {
            theConfiguration.RegisterMissingService<object>();

            theConfiguration.MissingServices.ShouldHaveTheSameElementsAs(new MissingService(typeof(object)));
        }

        [Test]
        public void valid_without_errors()
        {
            theConfiguration.IsValid().ShouldBeTrue();
        }

        [Test]
        public void invalid_with_errors()
        {
            theConfiguration.RegisterError(new FakeError());
            theConfiguration.IsValid().ShouldBeFalse();
        }

        public class FakeError : BottleConfigurationError
        {
            
        }
    }
}