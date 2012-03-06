using System;
using Bottles.Deployment.Configuration;
using FubuCore.Configuration;
using NUnit.Framework;
using FubuTestingSupport;

namespace Bottles.Tests
{
    [TestFixture]
    public class EnvironmentSettingsTester
    {
        private EnvironmentSettings theEnvironmentSettings;

        [SetUp]
        public void SetUp()
        {
            theEnvironmentSettings = new EnvironmentSettings();
        }

        [Test]
        public void read_text_with_no_equals()
        {
            Exception<Exception>.ShouldBeThrownBy(() =>
            {
                theEnvironmentSettings.Data.Read("something");
            });
        }

        [Test]
        public void read_text_with_too_many_equals()
        {
            theEnvironmentSettings.Data.Read("something=else=more");

            theEnvironmentSettings.Data["something"].ShouldEqual("else=more");
        }

        [Test]
        public void read_text_with_equals_and_only_one_dot()
        {
            theEnvironmentSettings.Data.Read("arg.1=value");
            theEnvironmentSettings.Data["arg.1"].ShouldEqual("value");
        }

        [Test]
        public void environment_settings_must_be_categorized_as_environment()
        {
            theEnvironmentSettings.Data.Category.ShouldEqual(SettingCategory.environment);
        }

        [Test]
        public void read_simple_value()
        {
            theEnvironmentSettings.Data.Read("A=B");
            theEnvironmentSettings.Data.Read("C=D");

            theEnvironmentSettings.Data["A"].ShouldEqual("B");
            theEnvironmentSettings.Data["C"].ShouldEqual("D");
        }

        [Test]
        public void read_host_directive()
        {
            theEnvironmentSettings.Data.Read("Host1.OneDirective.Name=Jeremy");
            theEnvironmentSettings.Data.Read("Host1.OneDirective.Age=45");
            theEnvironmentSettings.Data.Read("Host2.OneDirective.Name=Tom");

            theEnvironmentSettings.DataForHost("Host1")["OneDirective.Name"].ShouldEqual("Jeremy");
            theEnvironmentSettings.DataForHost("Host2")["OneDirective.Name"].ShouldEqual("Tom");
            theEnvironmentSettings.DataForHost("Host1")["OneDirective.Age"].ShouldEqual("45");
        }

        [Test]
        public void the_environment_settings_are_categorized_as_environment()
        {
            theEnvironmentSettings.DataForHost("Host1").Category.ShouldEqual(SettingCategory.environment);
            theEnvironmentSettings.DataForHost("Host2").Category.ShouldEqual(SettingCategory.environment);
        }
    }
}