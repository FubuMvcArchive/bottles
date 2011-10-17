using System;
using System.Diagnostics;
using System.Reflection;
using Bottles.Diagnostics;
using FubuCore.Util;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;
using System.Collections.Generic;
using Rhino.Mocks;

namespace Bottles.Tests
{
    [TestFixture]
    public class PackagingDependencyProcessorTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            thePackages = new Cache<string, StubPackage>(key => new StubPackage(key));
            theDiagnostics = new StubPackageDiagnostics();
            
        }

        #endregion

        private Cache<string, StubPackage> thePackages;
        private StubPackageDiagnostics theDiagnostics;

        private void hasPackage(string name)
        {
            thePackages.FillDefault(name);
        }

        private void theOrderedPackageNamesShouldBe(params string[] names)
        {
            var theActualOrder = theDependencyProcessor.OrderedPackages().Select(x => x.Name);
            try
            {
                theActualOrder
                    .ShouldHaveTheSameElementsAs(names);
            }
            catch (Exception)
            {
                theActualOrder.Each(x => Debug.WriteLine(x));
                throw;
            }
        }

        public PackageDependencyProcessor theDependencyProcessor
        {
            get { return new PackageDependencyProcessor(thePackages); }
        }

        [Test]
        public void order_by_name_in_the_absence_of_no_other_information()
        {
            hasPackage("C");
            hasPackage("B");
            hasPackage("A");

            theOrderedPackageNamesShouldBe("A", "B", "C");
        }

        [Test]
        public void log_missing_dependency_marks_a_failure()
        {
            var log = MockRepository.GenerateMock<IPackageLog>();
            log.LogMissingDependency("A1");

            log.AssertWasCalled(x => x.MarkFailure("Missing required Bottle/Package dependency named 'A1'"));
        }

        [Test]
        public void log_nothing_without_any_missing_dependencies()
        {
            hasPackage("C");
            hasPackage("B");
            hasPackage("A");
            
            theDependencyProcessor.LogMissingPackageDependencies(theDiagnostics);
            theDiagnostics.HasMessages().ShouldBeFalse();
        }

        [Test]
        public void log_nothing_if_an_optional_dependency_is_missing()
        {
            hasPackage("C");
            hasPackage("B");
            hasPackage("A");

            thePackages["A"].OptionalDependency("D");

            theDependencyProcessor.LogMissingPackageDependencies(theDiagnostics);
            theDiagnostics.HasMessages().ShouldBeFalse();
        }

        [Test]
        public void log_all_missing_mandatory_dependencies()
        {
            thePackages["A"].MandatoryDependency("A1");
            thePackages["A"].MandatoryDependency("A2");
            thePackages["B"].MandatoryDependency("B1");
            thePackages["C"].MandatoryDependency("B1");

            theDependencyProcessor.LogMissingPackageDependencies(theDiagnostics);

            theDiagnostics.LogFor(thePackages["A"]).AssertWasCalled(x => x.LogMissingDependency("A1"));
            theDiagnostics.LogFor(thePackages["A"]).AssertWasCalled(x => x.LogMissingDependency("A2"));
            theDiagnostics.LogFor(thePackages["B"]).AssertWasCalled(x => x.LogMissingDependency("B1"));
            theDiagnostics.LogFor(thePackages["C"]).AssertWasCalled(x => x.LogMissingDependency("B1"));
        }


        [Test]
        public void dependency_ordering_impacts_sorting()
        {
            thePackages["A"].OptionalDependency("B");
            thePackages["B"].MandatoryDependency("C");
            hasPackage("C");

            theOrderedPackageNamesShouldBe("C", "B", "A");
        }

        [Test]
        public void orders_alphabetically_in_the_absence_of_other_dependency_rules_but_dependency_rules_win()
        {
            thePackages["A"].OptionalDependency("B");
            thePackages["B"].MandatoryDependency("C");
            hasPackage("D");
            hasPackage("E");
            hasPackage("C");

            theOrderedPackageNamesShouldBe("C", "D", "E", "B", "A");
        }

        [Test]
        public void can_sort_with_an_optional_dependency_that_does_not_exist()
        {
            thePackages["A"].OptionalDependency("B");
            thePackages["B"].OptionalDependency("C");
            hasPackage("D");
            hasPackage("E");
            // C does not exist
            //hasPackage("C"); 

            theOrderedPackageNamesShouldBe("D", "E", "B", "A");
        }
    }

    public class StubPackageDiagnostics : IPackagingDiagnostics
    {
        private readonly Cache<object, IPackageLog> _logs = new Cache<object, IPackageLog>(o => MockRepository.GenerateMock<IPackageLog>());

        public void LogObject(object target, string provenance)
        {
            throw new NotImplementedException();
        }

        public void LogPackage(IPackageInfo package, IPackageLoader loader)
        {
            throw new NotImplementedException();
        }

        public void LogBootstrapperRun(IBootstrapper bootstrapper, IEnumerable<IActivator> activators)
        {
            throw new NotImplementedException();
        }

        public void LogAssembly(IPackageInfo package, Assembly assembly, string provenance)
        {
            throw new NotImplementedException();
        }

        public void LogDuplicateAssembly(IPackageInfo package, string assemblyName)
        {
            throw new NotImplementedException();
        }

        public void LogAssemblyFailure(IPackageInfo package, string fileName, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void LogExecution(object target, Action continuation)
        {
            throw new NotImplementedException();
        }

        public IPackageLog LogFor(object target)
        {
            return _logs[target];
        }

        public bool HasMessages()
        {
            return _logs.Any();
        }
    }
}