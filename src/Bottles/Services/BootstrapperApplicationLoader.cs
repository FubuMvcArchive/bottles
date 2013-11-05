using System;
using System.Collections.Generic;

namespace Bottles.Services
{
    public class BootstrapperApplicationLoader<T> : IApplicationLoader, IDisposable where T : IBootstrapper, new()
    {
        private WrappedBootstrapper _wrappedBootstrapper;

        public IDisposable Load()
        {
            _wrappedBootstrapper = new WrappedBootstrapper(new T());

            PackageRegistry.LoadPackages(x => {
                x.Bootstrapper(_wrappedBootstrapper);
                x.Loader(new BottleServicePackageLoader());
            });

            return this;
        }

        public void Dispose()
        {
            _wrappedBootstrapper.BottleServices().Each(x => x.Stop());
        }

        public override string ToString()
        {
            return new T().ToString();
        }
    }
}