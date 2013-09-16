using System;
using System.Net.Sockets;

namespace Bottles.Services
{
    public interface IApplicationLoader
    {
        IDisposable Load();
    }

    public class ApplicationLoader<TSource, TApplication, TRuntime> : IApplicationLoader
        where TSource : IApplicationSource<TApplication, TRuntime>, new()
        where TApplication : IApplication<TRuntime> 
        where TRuntime : IDisposable
    {
        public IDisposable Load()
        {
            return new TSource().BuildApplication().Bootstrap();
        }
    }

    public interface IApplication<T> where T : IDisposable
    {
        T Bootstrap();
    }

    public interface IApplicationSource<TApplication, TRuntime>
        where TApplication : IApplication<TRuntime>
        where TRuntime : IDisposable
    {
        TApplication BuildApplication();
    }

    public class DefaultBottleApplication : IApplication<DefaultBottleApplication>,
        IApplicationSource<DefaultBottleApplication, DefaultBottleApplication>, IDisposable
    {
        private BottleServiceRunner _services;

        public DefaultBottleApplication Bootstrap()
        {
            var facility = new BottlesServicePackageFacility();
            PackageRegistry.LoadPackages(x => x.Facility(facility));


            PackageRegistry.AssertNoFailures();

            _services = facility.Aggregator.ServiceRunner();
            _services.Start();

            return this;
        }

        public DefaultBottleApplication BuildApplication()
        {
            return this;
        }

        public void Dispose()
        {
            _services.Stop();
        }
    }
}