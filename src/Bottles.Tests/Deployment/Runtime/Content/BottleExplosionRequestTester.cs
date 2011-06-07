using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Tests.Deployment.Runtime.Content
{
    [TestFixture]
    public class BottleExplosionRequestTester
    {
        [Test]
        public void copy_behavior_is_overwrite_by_default()
        {
            new BottleExplosionRequest().CopyBehavior.ShouldEqual(CopyBehavior.overwrite);
            new BottleExplosionRequest(new PackageLog()).CopyBehavior.ShouldEqual(CopyBehavior.overwrite);
        }
    }
}