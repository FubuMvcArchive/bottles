using Bottles.PackageLoaders.Assemblies;
using NUnit.Framework;
using Rhino.Mocks;
using FubuTestingSupport;

namespace Bottles.Tests.PackageLoaders.Assemblies
{
    [TestFixture]
    public class AssemblyPackageInfoTester
    {
        [Test]
        public void assembly_package_adds_an_assembly_target_for_the_assembly()
        {
            // This behavior is crucial.  If you add the assembly the wrong way, 
            // Bottles tries to copy the assembly to the application bin directory

            var theAssembly = GetType().Assembly;

            var package = new AssemblyPackageInfo(theAssembly);

            var registration = MockRepository.GenerateMock<IAssemblyRegistration>();
            package.LoadAssemblies(registration);

            registration.AssertWasCalled(x => x.Use(theAssembly));

        }

        [Test]
        public void by_default_the_package_name_of_an_assembly_package_is_just_the_assembly_name()
        {
            var theAssembly = GetType().Assembly;

            var package = new AssemblyPackageInfo(theAssembly);

            package.Name.ShouldEqual(theAssembly.GetName().Name);
        }
    }
}