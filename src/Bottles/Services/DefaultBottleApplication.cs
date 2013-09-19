using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bottles.Services.Messaging;

namespace Bottles.Services
{
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