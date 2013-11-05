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
    public class BottlingDependencyProcessorTester : 
        InteractionContext<BottleDependencyProcessor>
    {
        IList<StubBottle> thePackages;
        StubBottleDiagnostics theDiagnostics;

        protected override void beforeEach()
        {
            thePackages = new List<StubBottle>();

            hasPackage("C");
            hasPackage("B");
            hasPackage("A");

            theDiagnostics = new StubBottleDiagnostics();
        }
       

        [Test]
        public void order_by_name_in_the_absence_of_no_other_information()
        {
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
            ClassUnderTest.LogMissingPackageDependencies(theDiagnostics);
            theDiagnostics.HasMessages().ShouldBeFalse();
        }

        [Test]
        public void log_nothing_if_an_optional_dependency_is_missing()
        {
            hasPackage("A").OptionalDependency("D");

            ClassUnderTest.LogMissingPackageDependencies(theDiagnostics);
            theDiagnostics.HasMessages().ShouldBeFalse();
        }

        [Test]
        public void log_all_missing_mandatory_dependencies()
        {
            hasPackage("A").MandatoryDependency("A1");
            hasPackage("A").MandatoryDependency("A2");
            hasPackage("B").MandatoryDependency("B1");
            hasPackage("C").MandatoryDependency("B1");

            ClassUnderTest.LogMissingPackageDependencies(theDiagnostics);

            theDiagnostics.LogFor(hasPackage("A")).AssertWasCalled(x => x.LogMissingDependency("A1"));
            theDiagnostics.LogFor(hasPackage("A")).AssertWasCalled(x => x.LogMissingDependency("A2"));
            theDiagnostics.LogFor(hasPackage("B")).AssertWasCalled(x => x.LogMissingDependency("B1"));
            theDiagnostics.LogFor(hasPackage("C")).AssertWasCalled(x => x.LogMissingDependency("B1"));
        }


        [Test]
        public void dependency_ordering_impacts_sorting()
        {
            hasPackage("A").OptionalDependency("B");
            hasPackage("B").MandatoryDependency("C");
            hasPackage("C");

            theOrderedPackageNamesShouldBe("C", "B", "A");
        }

        [Test]
        public void orders_alphabetically_in_the_absence_of_other_dependency_rules_but_dependency_rules_win()
        {
            hasPackage("A").OptionalDependency("B");
            hasPackage("B").MandatoryDependency("C");
            hasPackage("D");
            hasPackage("E");
            hasPackage("C");

            theOrderedPackageNamesShouldBe("C", "D", "E", "B", "A");
        }

        [Test]
        public void can_sort_with_an_optional_dependency_that_does_not_exist()
        {
            hasPackage("A").OptionalDependency("B");
            hasPackage("B").OptionalDependency("Z");
            hasPackage("D");
            hasPackage("E");
            // Z does not exist
            //hasPackage("Z"); 

            theOrderedPackageNamesShouldBe("C", "D", "E", "B", "A");
        }

        //helpers
        private StubBottle hasPackage(string name)
        {
            var x = new StubBottle(name);
            if(thePackages.Any(p=>p.Name.Equals(x.Name)))
            {
                return thePackages.First(p => p.Name.Equals(name));
            }

            thePackages.Add(x);
            Services.Inject<IPackageInfo>(x);
            return x;
        }

        private void theOrderedPackageNamesShouldBe(params string[] names)
        {
            var theActualOrder = ClassUnderTest.OrderedPackages().Select(x => x.Name);
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

    }

    public class StubBottleDiagnostics : IBottlingDiagnostics
    {
        private readonly Cache<object, IPackageLog> _logs = new Cache<object, IPackageLog>(o => MockRepository.GenerateMock<IPackageLog>());

        public void LogObject(object target, string provenance)
        {
        }

        public void LogPackage(IPackageInfo package, IPackageLoader loader)
        {
      
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
            continuation();
        }

        public void EachLog(Action<object, PackageLog> action)
        {
            throw new NotImplementedException();
        }

        public bool HasErrors()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<LogSubject> LogsForSubjectType<T>()
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

        public IEnumerable<IPackageLog> AllLogs()
        {
            return _logs.GetAll();
        }
    }
}