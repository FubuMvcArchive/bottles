using Bottles.PackageLoaders.Assemblies;
using NUnit.Framework;
using Rhino.Mocks;

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
    }
}