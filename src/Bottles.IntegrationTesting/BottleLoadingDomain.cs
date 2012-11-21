using System;

namespace Bottles.IntegrationTesting
{
    public class BottleLoadingDomain : IDisposable
    {
        private Lazy<AppDomain> _domain;
        private Lazy<BottleDomainProxy> _proxy; 

        public BottleLoadingDomain()
        {
            Recycle();
        }

        public BottleDomainProxy Proxy
        {
            get { return _proxy.Value; }
        }

        public void Recycle()
        {
            Dispose();

            _domain = new Lazy<AppDomain>(() =>
            {
                var setup = new AppDomainSetup
                            {
                                ApplicationName = "Bottles-Testing-" + Guid.NewGuid(),
                                ShadowCopyFiles = "true",
                                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
                            };

                return AppDomain.CreateDomain("Bottles-Testing", null, setup);
            });

            _proxy = new Lazy<BottleDomainProxy>(() =>
            {
                var proxyType = typeof(BottleDomainProxy);
                return (BottleDomainProxy)_domain.Value.CreateInstanceAndUnwrap(proxyType.Assembly.FullName, proxyType.FullName);
            });
        }

        public void Dispose()
        {
            if (_domain != null && _domain.IsValueCreated)
            {
                AppDomain.Unload(_domain.Value);
            }
        }
    }
}