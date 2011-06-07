using Bottles.Deployment;
using Bottles.Deployment.Configuration;
using Bottles.Deployment.Writing;
using FubuCore;
using NUnit.Framework;
using FubuTestingSupport;
using System.Linq;

namespace Bottles.Tests.Deployment
{
    [TestFixture]
    public class DeploymentSettingsTester
    {
        private FileSystem system;

        [SetUp]
        public void SetUp()
        {
            system = new FileSystem();
            system.CleanDirectory("firefly");
        }

        [Test]
        public void build_the_default_ctor()
        {
            setupValidDeploymentFolderAt("dir");

            //review there is a check inside of here
            var settings = new DeploymentSettings("dir");

            settings.BottlesDirectory.ShouldEqual("dir".AppendPath("bottles"));
            settings.RecipesDirectory.ShouldEqual("dir".AppendPath("recipes"));
            settings.EnvironmentFile().ShouldEqual("dir".AppendPath("environment.settings"));
            settings.TargetDirectory.ShouldEqual("dir".AppendPath("target"));
            settings.DeploymentDirectory.ShouldEqual("dir");

            settings.DeployersDirectory.ShouldEqual("dir".AppendPath("deployers"));

            settings.ProfilesDirectory.ShouldEqual(FileSystem.Combine("dir", ProfileFiles.ProfilesDirectory));

            settings.GetHost("x", "z").ShouldEqual("dir".AppendPath("recipes", "x", "z.host"));
            settings.GetRecipeDirectory("a").ShouldEqual("dir".AppendPath("recipes","a"));
        }

        private void setupValidDeploymentFolderAt(string name)
        {
            var pr = new DeploymentWriter(name);
            pr.Flush(FlushOptions.Wipeout);
        }

        private void writeEnvironmentFileTo(string directory)
        {
            system.WriteStringToFile(directory.AppendPath(EnvironmentSettings.EnvironmentSettingsFileName), "1");
        }

        private void writeProfileFileTo(string directory, string profileName)
        {
            var filename = profileName + "." + ProfileFiles.ProfileExtension;
            filename = directory.AppendPath(ProfileFiles.ProfilesDirectory).AppendPath(filename);

            system.WriteStringToFile(filename, "a");
        }

        [Test]
        public void find_the_environment_file_defaults_to_the_main_directory_if_it_cannot_be_found()
        {
            var settings = new DeploymentSettings("firefly");
            var defaultEnvironmentFile = "firefly".AppendPath(EnvironmentSettings.EnvironmentSettingsFileName);
            settings.EnvironmentFile().ShouldEqual(defaultEnvironmentFile);
        }

        [Test]
        public void find_the_environment_file_with_no_included_folders()
        {
            var settings = new DeploymentSettings("firefly");
            var defaultEnvironmentFile = "firefly".AppendPath(EnvironmentSettings.EnvironmentSettingsFileName);
            system.WriteStringToFile(defaultEnvironmentFile, "something");

            settings.Directories.Count().ShouldEqual(1);

            settings.EnvironmentFile().ShouldEqual("firefly".AppendPath(EnvironmentSettings.EnvironmentSettingsFileName));
        }

        [Test]
        public void find_environment_file_with_included_folders_should_still_choose_the_main_environment_settings_file()
        {
            var settings = new DeploymentSettings("firefly");
            var defaultEnvironmentFile = "firefly".AppendPath(EnvironmentSettings.EnvironmentSettingsFileName);
            system.WriteStringToFile(defaultEnvironmentFile, "something");

            writeEnvironmentFileTo("a");
            settings.AddImportedFolder("a");
            settings.EnvironmentFile().ShouldEqual("firefly".AppendPath(EnvironmentSettings.EnvironmentSettingsFileName));
        }

        [Test]
        public void find_environment_file_from_included_folders_when_it_is_not_in_the_root()
        {
            var settings = new DeploymentSettings("firefly");

            writeEnvironmentFileTo("a");
            settings.AddImportedFolder("a");
            settings.EnvironmentFile().ShouldEqual("a".AppendPath(EnvironmentSettings.EnvironmentSettingsFileName));

        }

      
    }
}