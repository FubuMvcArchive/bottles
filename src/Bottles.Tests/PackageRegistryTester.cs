using System;
using System.Collections.Generic;
using System.Linq;
using Bottles.Diagnostics;
using Bottles.Environment;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Tests
{
    [TestFixture]
    public class PackageRegistryTester
    {
        [Test]
        public void assert_failures_with_no_failures()
        {
            PackageRegistry.LoadPackages(x =>
            {
                x.Bootstrap("Cool stuff", log =>
                {
                    return new List<IActivator>();
                });
            });

            PackageRegistry.AssertNoFailures();
        }

        [Test]
        public void assert_failures_blows_up_when_anything_in_the_diagnostics_has_a_problem()
        {
            PackageRegistry.LoadPackages(x =>
            {
                x.Bootstrap("Cool stuff", log =>
                {
                    throw new ApplicationException("You shall not pass");
                });
            });

            Exception<ApplicationException>.ShouldBeThrownBy(() =>
            {
                PackageRegistry.AssertNoFailures();
            }).Message.ShouldContain("You shall not pass");
        }

        [Test]
        public void should_run_activators()
        {
            bool ran = false;
            PackageRegistry.LoadPackages(x =>
            {
                x.Bootstrap("Cool stuff", log =>
                {
                    return new List<IActivator>(){new LambdaActivator("x",()=>
                    {
                        ran = true;
                    })};
                });
            });
            ran.ShouldBeTrue();
        }


        [Test]
        public void should_NOT_run_activators()
        {
            bool hasNotRun = true;
            PackageRegistry.LoadPackages(x =>
            {
                x.Bootstrap("Cool stuff", log =>
                {
                    return new List<IActivator>(){new LambdaActivator("x",()=>
                    {
                        hasNotRun = false;
                    })};
                });
            }, runActivators:false);
            hasNotRun.ShouldBeTrue();
        }

        [Test]
        public void should_run_all_of_the_environment_checks_in_bootstrapping()
        {
            var activator = new FakeActivator();
            activator.SucceedWith("good");
            activator.SucceedWith("better");
            activator.SucceedWith("best");

            PackageRegistry.LoadPackages(x => {
                x.Activator(activator);
            });

            activator.Requirements().OfType<FakeEnvironmentReqirement>()
                .Select(x => x.WasCalled)
                .ShouldHaveTheSameElementsAs(true, true, true);

           
        }

        [Test]
        public void activators_should_run_if_all_environment_checks_pass()
        {
            var activator = new FakeActivator();
            activator.SucceedWith("good");
            activator.SucceedWith("better");
            activator.SucceedWith("best");

            var activator2 = new FakeActivator();
            var activator3 = new FakeActivator();

            PackageRegistry.LoadPackages(x =>
            {
                x.Activator(activator);
                x.Activator(activator2);
                x.Activator(activator3);
            });

            PackageRegistry.AssertNoFailures();

            activator.WasActivated.ShouldBeTrue();
            activator2.WasActivated.ShouldBeTrue();
            activator3.WasActivated.ShouldBeTrue();
        }

        [Test]
        public void activators_do_not_run_if_any_environment_check_fails()
        {
            var activator = new FakeActivator();
            activator.SucceedWith("good");
            activator.SucceedWith("better");
            activator.FailWith("worst");

            var activator2 = new FakeActivator();
            var activator3 = new FakeActivator();

            PackageRegistry.LoadPackages(x =>
            {
                x.Activator(activator);
                x.Activator(activator2);
                x.Activator(activator3);
            });

            activator.WasActivated.ShouldBeFalse();
            activator2.WasActivated.ShouldBeFalse();
            activator3.WasActivated.ShouldBeFalse();
        }

        [Test, Explicit("wonky in end to end tests")]
        public void environment_checks_are_logged_in_diagnostic_log()
        {
            var activator = new FakeActivator();
            activator.SucceedWith("good");
            activator.SucceedWith("better");
            activator.FailWith("worst");

            var activator2 = new FakeActivator();
            var activator3 = new FakeActivator();

            PackageRegistry.LoadPackages(x =>
            {
                x.Activator(activator);
                x.Activator(activator2);
                x.Activator(activator3);
            });

            PackageRegistry.Diagnostics.LogsForSubjectType<IEnvironmentRequirement>()
                .Select(x => x.Subject)
                .ShouldHaveTheSameElementsAs(activator.Requirements());
        }
    }

    public class FakeActivator : IActivator, IEnvironmentRequirements
    {
        private readonly IList<IEnvironmentRequirement> _requirements = new List<IEnvironmentRequirement>();
        public bool WasActivated;

        public void FailWith(string message)
        {
            _requirements.Add(new FakeEnvironmentReqirement(message, false));
        }

        public void SucceedWith(string message)
        {
            _requirements.Add(new FakeEnvironmentReqirement(message, true));
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            WasActivated = true;
        }

        public IEnumerable<IEnvironmentRequirement> Requirements()
        {
            return _requirements;
        }
    }

    public class FakeEnvironmentReqirement : IEnvironmentRequirement
    {
        private readonly string _message;
        private readonly bool _success;

        public FakeEnvironmentReqirement(string message, bool success)
        {
            _message = message;
            _success = success;
        }

        public string Describe()
        {
            return _message;
        }

        public void Check(IPackageLog log)
        {
            WasCalled = true;

            if (!_success)
            {
                log.MarkFailure(_message);
            }
            else
            {
                log.Trace(_message);
            }
        }

        public bool WasCalled;
    }
}