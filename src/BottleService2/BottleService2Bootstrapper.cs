using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using Bottles.Tests.Harness;

namespace BottleService2
{
    public class BottleService2Bootstrapper : IBootstrapper
    {
        public IEnumerable<IActivator> Bootstrap(IPackageLog log)
        {
            yield return new FileWriterActivator("2a");
            yield return new FileWriterActivator("2b");
        }
    }
}