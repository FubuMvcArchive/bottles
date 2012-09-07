using Bottles.Configuration;
using Bottles.Diagnostics;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Bottles.Tests.Configuration
{
    [TestFixture]
    public class when_asserting_the_bottle_configuration
    {
        private IBottleConfigurationRule r1;
        private IBottleConfigurationRule r2;
        private AssertBottleConfiguration theInstaller;

        [SetUp]
        public void SetUp()
        {
            r1 = MockRepository.GenerateMock<IBottleConfigurationRule>();
            r2 = MockRepository.GenerateMock<IBottleConfigurationRule>();

            theInstaller = new AssertBottleConfiguration("Test", new[] { r1, r2 });
        }

        [Test]
        public void each_rule_gets_evaluated()
        {
            theInstaller.Activate(new IPackageInfo[0], new PackageLog());

            r1.AssertWasCalled(x => x.Evaluate(Arg<BottleConfiguration>.Is.NotNull));
            r2.AssertWasCalled(x => x.Evaluate(Arg<BottleConfiguration>.Is.NotNull));
        }

        [Test]
        public void the_configuration_exception_is_thrown_when_the_configuration_is_not_valid()
        {
            r1.Stub(x => x.Evaluate(Arg<BottleConfiguration>.Is.NotNull))
                .WhenCalled(mi => mi.Arguments[0].As<BottleConfiguration>().RegisterError(new MissingService(typeof(object))));

            Exception<BottleConfigurationException>
                .ShouldBeThrownBy(() => theInstaller.Activate(new IPackageInfo[0], new PackageLog()));
        }
    }
}