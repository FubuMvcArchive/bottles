using System.Collections.Generic;
using System.Linq;
using Bottles.Diagnostics;

namespace Bottles.Services
{
    public class WrappedBootstrapper : IBootstrapper
    {
        private readonly IBootstrapper _inner;
        private IEnumerable<BottleService> _services;

        public WrappedBootstrapper(IBootstrapper inner)
        {
            _inner = inner;
        }

        public IEnumerable<IActivator> Bootstrap(IPackageLog log)
        {
            _services = _inner.Bootstrap(log).Select(x => new BottleService(x, log));
            _services.Each(x => x.Start());

            return new IActivator[0];
        }

        public IEnumerable<IBottleService> BottleServices()
        {
            return _services;
        }
    }
}