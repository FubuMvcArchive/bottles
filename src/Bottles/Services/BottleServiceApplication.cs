using System;
using System.Collections.Generic;
using Bottles.Diagnostics;
using FubuCore;
using System.Linq;

namespace Bottles.Services
{
    public static class BottleServiceApplication
    {
        [SkipOverForProvenance]
        public static IApplicationLoader FindLoader(string bootstrapperType)
        {
            // TODO -- do something w/ the custom bootstrapper!
            // TODO -- go look for IApplicationSource & IApplicationLoader

            return new DefaultBottleApplication();

        }

        public static bool IsLoaderTypeCandidate(Type type)
        {
            if (!type.IsConcreteWithDefaultCtor()) return false;

            if (type.Closes(typeof (IApplicationSource<,>))) return true;

            return type.CanBeCastTo<IApplicationLoader>();
        }

        public static IApplicationLoader BuildApplicationLoader(Type type)
        {
            var loaderType = determineLoaderType(type);

            return Activator.CreateInstance(loaderType).As<IApplicationLoader>();
        }

        private static Type determineLoaderType(Type type)
        {
            var @interface = type.FindInterfaceThatCloses(typeof (IApplicationSource<,>));
            if (@interface == null) return type;

            var genericArguments = @interface.GetGenericArguments();
            return typeof (ApplicationLoader<,,>)
                .MakeGenericType(type, genericArguments.First(), genericArguments.Last());
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