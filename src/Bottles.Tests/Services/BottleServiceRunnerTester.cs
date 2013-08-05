using System.Collections.Generic;
using Bottles.Diagnostics;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Services.Tests
{
    [TestFixture]
    public class BottleServiceRunnerTester
    {
        private RecordingService s1;
        private RecordingService s2;
        private IPackageLog theLog;
        private BottleServiceRunner theRunner;

        [SetUp]
        public void SetUp()
        {
            s1 = new RecordingService();
            s2 = new RecordingService();
            theLog = new PackageLog();

            theRunner = new BottleServiceRunner(new[] { new BottleService(s1, theLog), new BottleService(s2, theLog)  });
        }

        [Test]
        public void starts_all_the_services()
        {
            theRunner.Start();

            s1.Activated.ShouldBeTrue();
            s2.Activated.ShouldBeTrue();
        }

        [Test]
        public void stops_all_the_services()
        {
            theRunner.Stop();

            s2.Deactivated.ShouldBeTrue();
            s2.Deactivated.ShouldBeTrue();
        }
    }

    public class RecordingService : IActivator, IDeactivator
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