using System.Diagnostics;
using Bottles.Deployment;
using NUnit.Framework;

namespace Bottles.Tests.Deployment
{
    [TestFixture]
    public class ProcessRunnerTester
    {
        [Test]
        public void ShouldntBeAnyExceptionsIfTheProcessHasExited()
        {
            var pr = new ProcessRunner();
            pr.Run(new ProcessStartInfo("ping", "google.com"));
        }
    }
}