using System;
using System.Reflection;
using System.Threading;
using Bottles.Diagnostics;
using FubuTestingSupport;
using NUnit.Framework;
using System.Linq;
using FubuCore;

namespace Bottles.Tests
{
    [TestFixture]
    public class BottlingDiagnosticsTester
    {
        private BottlingDiagnostics diagnostics;

        [SetUp]
        public void SetUp()
        {
            diagnostics = new BottlingDiagnostics(new LoggingSession());
        }

        [Test]
        public void log_an_object_creates_a_log_file()
        {
            var loader = new StubBottleLoader("a", "b");
            diagnostics.LogObject(loader, "registered at XYZ");

            diagnostics.LogFor(loader).Provenance.ShouldEqual("registered at XYZ");
        }

        [Test]
        public void log_an_object_sticks_the_ToString_value_of_the_object_on_to_the_description()
        {
            var loader = new StubBottleLoader("a", "b");
            diagnostics.LogObject(loader, "registered at XYZ");

            diagnostics.LogFor(loader).Description.ShouldEqual(loader.ToString());
        }
        
        [Test]
        public void log_execution_happy_path()
        {
            var loader = new StubBottleLoader("a", "b");
            diagnostics.LogObject(loader, "registered at XYZ");

            diagnostics.LogExecution(loader, () => Thread.Sleep(5));

            var log = diagnostics.LogFor(loader);
            log.Success.ShouldBeTrue();
            log.TimeInMilliseconds.ShouldBeGreaterThan(0);
        }

        [Test]
        public void log_execution_that_throws_exception()
        {
            var loader = new StubBottleLoader("a", "b");
            diagnostics.LogObject(loader, "registered at XYZ");

            diagnostics.LogExecution(loader, () =>
            {
                Thread.Sleep(5);
                throw new ApplicationException("not gonna happen");
            });

            var log = diagnostics.LogFor(loader);
            log.Success.ShouldBeFalse();
            log.TimeInMilliseconds.ShouldBeGreaterThan(0);
            log.FullTraceText().ShouldContain("not gonna happen");
        }

        [Test]
        public void log_an_assembly_failure()
        {
            var package = new StubBottle("a");
            var exception = new ApplicationException("didn't work");
            var theFileNameOfTheAssembly = "assembly.dll";
            diagnostics.LogAssemblyFailure(package, theFileNameOfTheAssembly, exception);

            var log = diagnostics.LogFor(package);

            log.Success.ShouldBeFalse();
            log.FullTraceText().Contains(exception.ToString()).ShouldBeTrue();

            log.FullTraceText().ShouldContain("Failed to load assembly at '{0}'".ToFormat(theFileNameOfTheAssembly));
        }

        [Test]
        public void log_a_duplicate_assembly()
        {
            var package = new StubBottle("a");
            diagnostics.LogDuplicateAssembly(package, "Duplicate.Assembly");

            diagnostics.LogFor(package).FullTraceText().ShouldContain("Assembly 'Duplicate.Assembly' was ignored because it is already loaded");
        }
    }

    [TestFixture]
    public class when_logging_a_package
    {
        private BottlingDiagnostics diagnostics;
        private StubBottleLoader loader;
        private StubBottle _bottle;

        [SetUp]
        public void SetUp()
        {
            diagnostics = new BottlingDiagnostics(new LoggingSession());

            loader = new StubBottleLoader("a", "b");
            _bottle = new StubBottle("a");

            diagnostics.LogPackage(_bottle, loader);
        }

        [Test]
        public void the_log_for_the_package_should_show_the_provenance_from_the_loader()
        {
            diagnostics.LogFor(_bottle).Provenance.ShouldEqual("Loaded by " + loader.ToString());
        }

        [Test]
        public void the_package_should_be_a_child_of_the_loader()
        {
            diagnostics.LogFor(loader).FindChildren<IBottleInfo>().Single().ShouldBeTheSameAs(_bottle);
        }


    }

    [TestFixture]
    public class when_logging_a_bootstrapper_run
    {
        private BottlingDiagnostics diagnostics;
        private StubActivator activator1;
        private StubActivator activator2;
        private StubActivator activator3;
        private StubBootstrapper bootstrapper;

        [SetUp]
        public void SetUp()
        {
            diagnostics = new BottlingDiagnostics(new LoggingSession());

            activator1 = new StubActivator();
            activator2 = new StubActivator();
            activator3 = new StubActivator();

            bootstrapper = new StubBootstrapper("Boot1", activator1, activator2, activator3);

            diagnostics.LogBootstrapperRun(bootstrapper, bootstrapper.Bootstrap(null));
        }

        [Test]
        public void each_activator_should_have_the_provenance()
        {
            diagnostics.LogFor(activator1).Provenance.ShouldEqual("Loaded by Bootstrapper:  " + bootstrapper);
            diagnostics.LogFor(activator2).Provenance.ShouldEqual("Loaded by Bootstrapper:  " + bootstrapper);
            diagnostics.LogFor(activator3).Provenance.ShouldEqual("Loaded by Bootstrapper:  " + bootstrapper);
        }

        [Test]
        public void log_for_the_bootstrapper_should_have_all_the_activators_as_children()
        {
            diagnostics.LogFor(bootstrapper).FindChildren<IActivator>()
                .ShouldHaveTheSameElementsAs(activator1, activator2, activator3);
        }


    }

    [TestFixture]
    public class when_logging_an_assembly
    {
        private BottlingDiagnostics diagnostics;
        private StubBottle _bottle;
        private Assembly assembly;
        private string theProvenance;

        [SetUp]
        public void SetUp()
        {
            diagnostics = new BottlingDiagnostics(new LoggingSession());
            _bottle = new StubBottle("a");
            assembly = Assembly.GetExecutingAssembly();

            theProvenance = "from here";
            diagnostics.LogAssembly(_bottle, assembly, theProvenance);
        }

        [Test]
        public void the_assembly_should_have_the_provenance()
        {
            diagnostics.LogFor(assembly).Provenance.ShouldEqual(theProvenance);
        }

        [Test]
        public void the_assembly_should_be_registered_as_coming_from_the_package()
        {
            diagnostics.LogFor(_bottle)
                .FindChildren<Assembly>().Single().ShouldBeTheSameAs(assembly);
        }

        [Test]
        public void trace_for_the_package_should_say_the_assembly()
        {
            diagnostics.LogFor(_bottle).FullTraceText().ShouldContain("Loaded assembly '" + assembly.GetName().FullName + "' v1.0.0.0");
        }
    }
}