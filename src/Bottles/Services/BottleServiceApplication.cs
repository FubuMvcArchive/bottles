using System;
using System.Collections.Generic;
using Bottles.Diagnostics;
using FubuCore;
using System.Linq;

namespace Bottles.Services
{




    // TODO -- This gets fancier later
    public class BottleServiceApplication
    {
        [SkipOverForProvenance]
        public BottleServiceRunner Bootstrap(string bootstrapperType = null)
        {
            // TODO -- this is going to change to using IApplicationSource
            // Check if this is IApplicationLoader or closes IApplicationSource<,>
            if (bootstrapperType.IsNotEmpty())
            {
                var type = Type.GetType(bootstrapperType);
                var bootstrapper = Activator.CreateInstance(type).As<IBootstrapper>();

                var wrapped = new WrappedBootstrapper(bootstrapper);
                PackageRegistry.LoadPackages(x => x.Bootstrapper(wrapped));

                PackageRegistry.AssertNoFailures();

                return new BottleServiceRunner(wrapped.BottleServices());
            }

            var facility = new BottlesServicePackageFacility();
            PackageRegistry.LoadPackages(x => x.Facility(facility));
            

            PackageRegistry.AssertNoFailures();

            return facility.Aggregator.ServiceRunner();
        }

        public static bool IsLoaderTypeCandidate(Type type)
        {
            if (!type.IsConcreteWithDefaultCtor()) return false;

            if (type.Closes(typeof (IApplicationSource<,>))) return true;

            return type.CanBeCastTo<IApplicationLoader>();
        }
    }

    [MarkedForTermination("Going to eliminate.")]
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

            return new IActivator[0];
        }

        public IEnumerable<IBottleService> BottleServices()
        {
            return _services;
        }
    }
}