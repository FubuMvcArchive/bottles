using System;
using Bottles;
using Bottles.Diagnostics;
using Bottles.Services;
using Bottles.Tests.Harness;

namespace ApplicationLoaderService
{
    public class MyApplicationLoader : IApplicationLoader, IDisposable
    {
        private readonly FileWriterActivator _activator = new FileWriterActivator("loader");

        public IDisposable Load()
        {
            _activator.Activate(new IPackageInfo[0], new PackageLog());

            return this;
        }

        public void Dispose()
        {
            _activator.Deactivate(new PackageLog());
        }
    }
}