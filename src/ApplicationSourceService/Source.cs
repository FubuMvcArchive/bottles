using System;
using Bottles;
using Bottles.Diagnostics;
using Bottles.Services;
using Bottles.Tests.Harness;

namespace ApplicationSourceService
{
    public class Application : IApplication<IDisposable>, IDisposable
    {
        private readonly FileWriterActivator _activator = new FileWriterActivator("source");

        public IDisposable Bootstrap()
        {
            _activator.Activate(new IPackageInfo[0], new PackageLog());

            return this;
        }

        public void Dispose()
        {
            _activator.Deactivate(new PackageLog());
        }
    }

    public class GoodApplicationSource : IApplicationSource<Application, IDisposable>
    {
        public Application BuildApplication()
        {
            return new Application();
        }
    }
}