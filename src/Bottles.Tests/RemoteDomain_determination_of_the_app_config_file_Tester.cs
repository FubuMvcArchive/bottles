using System.IO;
using Bottles.Services.Remote;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Tests
{
    [TestFixture]
    public class RemoteDomain_determination_of_the_app_config_file_Tester
    {
        [Test]
        public void use_app_config_if_it_exists()
        {
            var fileSystem = new FileSystem();

            fileSystem.DeleteDirectory("Service");

            fileSystem.CreateDirectory("Service");
            fileSystem.CreateDirectory("Service", "bin");
            fileSystem.WriteStringToFile("Service".AppendPath("App.config"), "foo");

            var expression = new RemoteDomainExpression();
            expression.ServiceDirectory = "Service";
            Path.GetFileName(expression.Setup.ConfigurationFile).ShouldEqual("app.config");
        }

        [Test]
        public void use_web_config_if_it_exists()
        {
            var fileSystem = new FileSystem();

            fileSystem.DeleteDirectory("Service");

            fileSystem.CreateDirectory("Service");
            fileSystem.CreateDirectory("Service", "bin");
            fileSystem.WriteStringToFile("Service".AppendPath("Web.config"), "foo");

            var expression = new RemoteDomainExpression();
            expression.ServiceDirectory = "Service";
            Path.GetFileName(expression.Setup.ConfigurationFile).ShouldEqual("web.config");
        }

        [Test]
        public void stay_with_the_default_if_no_other_config_file_is_found()
        {
            var fileSystem = new FileSystem();

            fileSystem.DeleteDirectory("Service");

            fileSystem.CreateDirectory("Service");
            fileSystem.CreateDirectory("Service", "bin");

            var expression = new RemoteDomainExpression();
            expression.ServiceDirectory = "Service";
            Path.GetFileName(expression.Setup.ConfigurationFile).ShouldEqual("BottleServiceRunner.exe.config");
        }


    }
}