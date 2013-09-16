using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using Bottles.Tests.Harness;

namespace BottleService1
{
    public class BottleService1Bootstrapper : IBootstrapper
    {
        public IEnumerable<IActivator> Bootstrap(IPackageLog log)
        {
            yield return new FileWriterActivator("1");
        }
    }
}