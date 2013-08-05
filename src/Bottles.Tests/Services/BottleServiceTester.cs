using System;
using System.Collections.Generic;
using Bottles.Diagnostics;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Bottles.Services.Tests
{
    [TestFixture]
    public class BottleServiceTester
    {
        [Test]
        public void throws_if_not_a_deactivator()
        {
            var activatorOnly = MockRepository.GenerateMock<IActivator>();
            Exception<ArgumentException>
                .ShouldBeThrownBy(() => new BottleService(activatorOnly, new PackageLog()));
        }

        [Test]
        public void activates_on_start()
        {
            var inner = new InnerService();
            new BottleService(inner, new PackageLog()).Start();

            inner.Activated.ShouldBeTrue();
            inner.Deactivated.ShouldBeFalse();
        }

        [Test]
        public void deactivates_on_stop()
        {
            var inner = new InnerService();
            new BottleService(inner, new PackageLog()).Stop();

            inner.Activated.ShouldBeFalse();
            inner.Deactivated.ShouldBeTrue();
        }

        [Test]
        public void is_bottle_service()
        {
            BottleService.IsBottleService(typeof(InnerService)).ShouldBeTrue();
        }

        [Test]
        public void is_bottle_service_negative()
        {
            BottleService.IsBottleService(typeof(IActivator)).ShouldBeFalse();
            BottleService.IsBottleService(typeof(IDeactivator)).ShouldBeFalse();

            BottleService.IsBottleService(typeof(MarkerBottleInterface)).ShouldBeFalse();
        }

        private interface MarkerBottleInterface : IActivator, IDeactivator { }

        private class InnerService : IActivator, IDeactivator
        {
            public bool Activated { get; set; }
            public bool Deactivated { get; set; }

            public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
            {
                Activated = true;
            }

            public void Deactivate(IPackageLog log)
            {
                Deactivated = true;
            }
        }
    }
}