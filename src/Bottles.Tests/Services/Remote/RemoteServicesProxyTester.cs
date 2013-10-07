using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Bottles.Services.Remote;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Services.Tests.Remote
{
    [TestFixture]
    public class RemoteServicesProxyTester
    {
        [Test]
        public void set_properties_on_PackageRegistry_before_loading_the_loader()
        {
            SimpleLoader.Color = null;

            new RemoteServicesProxy().Start(typeof(SimpleLoader).AssemblyQualifiedName, new Dictionary<string, string>{{"Color", "Fuschia"}}, null );

            SimpleLoader.Color.ShouldEqual("Fuschia");
        }
    }

    public class SimpleLoader : IApplicationLoader, IDisposable
    {
        public static string Color;

        public IDisposable Load()
        {
            Color = PackageRegistry.Properties["Color"];
            return this;
        }

        public void Dispose()
        {
            
        }
    }
}