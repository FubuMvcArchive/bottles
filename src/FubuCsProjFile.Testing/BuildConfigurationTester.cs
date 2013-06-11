using NUnit.Framework;
using FubuTestingSupport;

namespace FubuCsProjFile.Testing
{
    [TestFixture]
    public class BuildConfigurationTester
    {
        [Test]
        public void create_from_text()
        {
            var config = new BuildConfiguration("		Debug|Mixed PlatformsKEY = Debug|Mixed Platforms");
            config.Key.ShouldEqual("Debug|Mixed PlatformsKEY");
            config.Value.ShouldEqual("Debug|Mixed Platforms");
        }
    }
}