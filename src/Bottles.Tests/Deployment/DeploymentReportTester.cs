using Bottles.Deployment.Diagnostics;
using Bottles.Diagnostics;
using NUnit.Framework;
using FubuTestingSupport;

namespace Bottles.Tests.Deployment
{
    [TestFixture]
    public class DeploymentReportTester
    {
        [Test]
        public void ShouldWriteFail()
        {
            var rpt = new DeploymentReport("HI");
            LoggingSession session = new LoggingSession();
            session.LogFor("hi").MarkFailure("BOOM!");
            session.HasErrors().ShouldBeTrue();
            rpt.WriteSuccessOrFail(session);
            rpt.Document.WriteToFile("bob.html");

        }
    }
}