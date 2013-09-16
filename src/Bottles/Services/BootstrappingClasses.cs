using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Bottles.Services.Messaging;

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

    public class DefaultBottleApplication : IApplicationLoader, IDisposable
    {
        private IEnumerable<IBottleService> _services;

        public IDisposable Load()
        {
            var facility = new BottlesServicePackageFacility();

            PackageRegistry.LoadPackages(x => x.Facility(facility));
            PackageRegistry.AssertNoFailures();

            _services = facility.Services();
            if (!_services.Any())
            {
                throw new ApplicationException("No services were detected.  Shutting down.");
            }

            var tasks = _services.Select(x => x.ToTask()).ToArray();

            _services.Each(x => EventAggregator.Messaging.AddListener(x));

            tasks.Each(x => x.Start());

            Task.WaitAll(tasks);

            return this;
        }

        public void Dispose()
        {
            _services.Each(x => x.Stop());
        }
    }
}