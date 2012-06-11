using Bottles.PackageLoaders.Assemblies;
using NUnit.Framework;
using Rhino.Mocks;

namespace Bottles.Tests
{
    [TestFixture]
    public class AssemblyTargetTester
    {
        [Test]
        public void if_you_have_the_assembly_directly_just_load()
        {
            var registration = MockRepository.GenerateMock<IAssemblyRegistration>();

            var theAssembly = GetType().Assembly;
            var target = AssemblyTarget.FromAssembly(theAssembly);

            target.Load(registration);

            registration.AssertWasCalled(x => x.Use(theAssembly));
            registration.AssertWasNotCalled(x => x.LoadFromFile(null, null), x => x.IgnoreArguments());
        }

        [Test]
        public void if_you_have_the_assembly_name_and_file()
        {
            var registration = MockRepository.GenerateMock<IAssemblyRegistration>();

            var theAssembly = GetType().Assembly;

            var target = new AssemblyTarget{
                AssemblyName = theAssembly.GetName().Name,
                FilePath = theAssembly.Location
            };

            target.Load(registration);

            registration.AssertWasNotCalled(x => x.Use(theAssembly));
            registration.AssertWasCalled(x => x.LoadFromFile(target.FilePath, target.AssemblyName));
        }
    }
}