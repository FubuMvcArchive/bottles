using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles.Services.Tests
{
    public class EmptyBootstrapper : IBootstrapper
    {
        public IEnumerable<IActivator> Bootstrap(IPackageLog log)
        {
            yield break;
        }
    }

    public class NonBottleServiceBootstrapper : IBootstrapper
    {
        public IEnumerable<IActivator> Bootstrap(IPackageLog log)
        {
            yield return new SimpleActivator();
        }

        public class SimpleActivator : IActivator
        {
            public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
            {
                throw new System.NotImplementedException();
            }
        }
    }

    public class StubServiceBootstrapper : IBootstrapper
    {
        public IEnumerable<IActivator> Bootstrap(IPackageLog log)
        {
            yield return new StubService();
        }
    }

    public class StubService : IActivator, IDeactivator
    {
        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
        }

        public void Deactivate(IPackageLog log)
        {
        }
    }
}