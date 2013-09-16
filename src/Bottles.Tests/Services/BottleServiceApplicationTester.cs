using System;
using Bottles.Services;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Tests.Services
{
    [TestFixture]
    public class BottleServiceApplicationTester
    {
        [Test]
        public void concrete_type_of_application_loader_with_default_ctor_is_a_candidate()
        {
            BottleServiceApplication.IsLoaderTypeCandidate(typeof (FakeApplicationLoader))
                .ShouldBeTrue();
        }

        [Test]
        public void application_loader_is_not_candidate_if_abstract()
        {
            BottleServiceApplication.IsLoaderTypeCandidate(typeof(AbstractApplicationLoader))
                .ShouldBeFalse();
        }

        [Test]
        public void application_loader_is_not_candidate_if_no_default_ctor()
        {
            BottleServiceApplication.IsLoaderTypeCandidate(typeof(TemplatedApplicationLoader))
                .ShouldBeFalse();
        }

        [Test]
        public void application_source_with_default_ctor_and_concrete_is_candidate()
        {
            BottleServiceApplication.IsLoaderTypeCandidate(typeof(GoodApplicationSource))
                .ShouldBeTrue();
        }

        [Test]
        public void application_source_with_no_default_ctor_is_not_candidate()
        {
            BottleServiceApplication.IsLoaderTypeCandidate(typeof(AbstractApplicationSource))
                .ShouldBeFalse();
        }

        [Test]
        public void application_source_without_default_ctor_is_not_a_candidate()
        {
            BottleServiceApplication.IsLoaderTypeCandidate(typeof(TemplatedApplicationSource))
                .ShouldBeFalse();
        }
    }

    public class TemplatedApplicationLoader : IApplicationLoader
    {
        public TemplatedApplicationLoader(string name)
        {
        }

        public IDisposable Load()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class AbstractApplicationLoader : IApplicationLoader
    {
        public IDisposable Load()
        {
            throw new NotImplementedException();
        }
    }

    public class FakeApplicationLoader : IApplicationLoader
    {
        public IDisposable Load()
        {
            throw new NotImplementedException();
        }
    }

    public class Application : IApplication<IDisposable>
    {
        public IDisposable Bootstrap()
        {
            throw new NotImplementedException();
        }
    }

    public class GoodApplicationSource : IApplicationSource<Application, IDisposable>
    {
        public Application BuildApplication()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class AbstractApplicationSource : IApplicationSource<Application, IDisposable>
    {
        public Application BuildApplication()
        {
            throw new NotImplementedException();
        }
    }

    public class TemplatedApplicationSource : IApplicationSource<Application, IDisposable>
    {
        public TemplatedApplicationSource(string name)
        {
        }

        public Application BuildApplication()
        {
            throw new NotImplementedException();
        }
    }

}