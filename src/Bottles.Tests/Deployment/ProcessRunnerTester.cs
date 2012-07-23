using System.Diagnostics;
using Bottles.Deployment;
using NUnit.Framework;
using FubuCore;

namespace Bottles.Tests.Deployment
{
    [TestFixture]
    public class ProcessRunnerTester
    {
        [Test]
        public void ShouldntBeAnyExceptionsIfTheProcessHasExited()
        {
            var pr = new ProcessRunner(new FileSystem());
            var x = pr.Run(new ProcessStartInfo("ping", "127.0.0.1"));
            x.OutputText.IsNotEmpty();
        }
    }
}