using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using Bottles.Tests.Harness;

namespace BottleService3
{
    public class BottleService3Bootstrapper : IBootstrapper
    {
        public IEnumerable<IActivator> Bootstrap(IPackageLog log)
        {
            yield return new FileWriterActivator("3");
        }
    }
}