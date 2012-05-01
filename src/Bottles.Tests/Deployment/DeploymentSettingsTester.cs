using System.IO;
using Bottles.Deployment;
using Bottles.Deployment.Commands;
using Bottles.Deployment.Configuration;
using Bottles.Deployment.Writing;
using FubuCore;
using Milkman;
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
            settings.EnvironmentFile.ShouldEqual("dir".AppendPath("environment.settings"));
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
        public void staging_directory()
        {
            setupValidDeploymentFolderAt("dir");

            //review there is a check inside of here
            var settings = new DeploymentSettings("dir");
            settings.StagingDirectory.ShouldEqual(Path.GetTempPath().AppendPath("bottles").AppendPath("staging"));
        }

        [Test]
        public void find_the_environment_file_defaults_to_the_main_directory_if_it_cannot_be_found()
        {
            var settings = new DeploymentSettings("firefly");
            var defaultEnvironmentFile = "firefly".AppendPath(EnvironmentSettings.EnvironmentSettingsFileName);
            settings.EnvironmentFile.ShouldEqual(defaultEnvironmentFile);
        }

        [Test]
        public void find_the_environment_file_with_no_included_folders()
        {
            var settings = new DeploymentSettings("firefly");
            var defaultEnvironmentFile = "firefly".AppendPath(EnvironmentSettings.EnvironmentSettingsFileName);
            system.WriteStringToFile(defaultEnvironmentFile, "something");

            settings.Directories.Count().ShouldEqual(1);

            settings.EnvironmentFile.ShouldEqual("firefly".AppendPath(EnvironmentSettings.EnvironmentSettingsFileName));
        }

        [Test]
        public void find_environment_file_with_included_folders_should_still_choose_the_main_environment_settings_file()
        {
            var settings = new DeploymentSettings("firefly");
            var defaultEnvironmentFile = "firefly".AppendPath(EnvironmentSettings.EnvironmentSettingsFileName);
            system.WriteStringToFile(defaultEnvironmentFile, "something");

            writeEnvironmentFileTo("a");
            settings.AddImportedFolder("a");
            settings.EnvironmentFile.ShouldEqual("firefly".AppendPath(EnvironmentSettings.EnvironmentSettingsFileName));
        }

        [Test]
        public void find_environment_file_from_included_folders_when_it_is_not_in_the_root()
        {
            writeEnvironmentFileTo("a");

            var settings = new DeploymentSettings("firefly");
            settings.AddImportedFolder("a");

            settings.EnvironmentFile.ShouldEqual("a".AppendPath(EnvironmentSettings.EnvironmentSettingsFileName));
        }

      
    }

    [TestFixture]
    public class DeploymentSettings_find_deployer_bottles
    {
        private DeploymentSettings theSettings;

        [SetUp]
        public void SetUp()
        {
            var fileSystem = new FileSystem();
            fileSystem.DeleteDirectory("hoth");
            fileSystem.CreateDirectory("hoth");

            new InitializeCommand().Execute(new InitializeInput()
            {
                DeploymentFlag = "hoth".AppendPath("deployment"),
                ForceFlag = true
            });

            fileSystem.DeleteDirectory("tatooine");
            fileSystem.CreateDirectory("tatooine");
            new InitializeCommand().Execute(new InitializeInput()
            {
                DeploymentFlag = "tatooine".AppendPath("deployment"),
                ForceFlag = true
            });

            theSettings = new DeploymentSettings("hoth".AppendPath("deployment"));
            theSettings.AddImportedFolder("tatooine".AppendPath("deployment"));
        }

        private void fileExistsAtFolder(string folder, string name)
        {
            new FileSystem().WriteStringToFile("hoth".AppendPath("deployment", folder, name), "something");
        }

        private void fileExistsAtOtherFolder(string folder, string name)
        {
            new FileSystem().WriteStringToFile("tatooine".AppendPath("deployment", folder, name), "something");
        }

        [Test]
        public void find_deployer_files_from_a_mix_of_imported_and_base_folder()
        {
            fileExistsAtFolder(ProfileFiles.BottlesDirectory, "a.zip");
            fileExistsAtFolder(ProfileFiles.BottlesDirectory, "b.zip");
            fileExistsAtFolder(ProfileFiles.DeployersDirectory, "c.zip");
            fileExistsAtFolder(ProfileFiles.DeployersDirectory, "d.zip");

            fileExistsAtOtherFolder(ProfileFiles.BottlesDirectory, "e.zip");
            fileExistsAtOtherFolder(ProfileFiles.BottlesDirectory, "f.zip");
            fileExistsAtOtherFolder(ProfileFiles.DeployersDirectory, "g.zip");
            fileExistsAtOtherFolder(ProfileFiles.DeployersDirectory, "h.zip");

            theSettings.DeployerBottleFiles().ShouldHaveTheSameElementsAs(
                theSettings.DeployersDirectory.AppendPath("c.zip"),
                theSettings.DeployersDirectory.AppendPath("d.zip"),
                "tatooine".AppendPath("deployment", ProfileFiles.DeployersDirectory, "g.zip"),
                "tatooine".AppendPath("deployment", ProfileFiles.DeployersDirectory, "h.zip")
                );
        }

        [Test]
        public void find_unique_deployer_names()
        {
            fileExistsAtFolder(ProfileFiles.BottlesDirectory, "a.zip");
            fileExistsAtFolder(ProfileFiles.BottlesDirectory, "b.zip");
            fileExistsAtFolder(ProfileFiles.DeployersDirectory, "c.zip");
            fileExistsAtFolder(ProfileFiles.DeployersDirectory, "d.zip");

            fileExistsAtOtherFolder(ProfileFiles.BottlesDirectory, "e.zip");
            fileExistsAtOtherFolder(ProfileFiles.BottlesDirectory, "f.zip");
            fileExistsAtOtherFolder(ProfileFiles.DeployersDirectory, "g.zip");
            fileExistsAtOtherFolder(ProfileFiles.DeployersDirectory, "h.zip"); 
            fileExistsAtOtherFolder(ProfileFiles.DeployersDirectory, "c.zip"); 
            fileExistsAtOtherFolder(ProfileFiles.DeployersDirectory, "d.zip");

            theSettings.DeployerBottleNames().ShouldHaveTheSameElementsAs("c", "d", "g", "h");
        }
    }

    [TestFixture]
    public class DeploymentSettings_find_bottle_file_IntegratedTester
    {
        private DeploymentSettings theSettings;

        [SetUp]
        public void SetUp()
        {
            var fileSystem = new FileSystem();
            fileSystem.DeleteDirectory("naboo");
            fileSystem.CreateDirectory("naboo");

            new InitializeCommand().Execute(new InitializeInput(){
                DeploymentFlag = "naboo".AppendPath("deployment"),
                ForceFlag = true
            });

            fileSystem.DeleteDirectory("other");
            fileSystem.CreateDirectory("other");
            new InitializeCommand().Execute(new InitializeInput()
            {
                DeploymentFlag = "other".AppendPath("deployment"),
                ForceFlag = true
            });

            theSettings = new DeploymentSettings("naboo".AppendPath("deployment"));
            theSettings.AddImportedFolder("other".AppendPath("deployment"));
        }

        private void fileExistsAtFolder(string folder, string name)
        {
            new FileSystem().WriteStringToFile("naboo".AppendPath("deployment", folder, name), "something");
        }

        private void fileExistsAtOtherFolder(string folder, string name)
        {
            new FileSystem().WriteStringToFile("other".AppendPath("deployment", folder, name), "something");
        }

        [Test]
        public void use_the_default_bottles_directory_name_if_the_bottle_exists_nowhere()
        {
            theSettings.BottleFileFor("a").ShouldEqual(theSettings.BottlesDirectory.AppendPath("a.zip"));  
        }

        [Test]
        public void use_the_bottles_directory_if_it_can_be_found()
        {
            fileExistsAtFolder(ProfileFiles.BottlesDirectory, "a.zip");
            theSettings.BottleFileFor("a").ShouldEqual(theSettings.BottlesDirectory.AppendPath("a.zip"));  

        }

        [Test]
        public void use_the_bottles_directory_in_other_if_it_can_be_found()
        {
            fileExistsAtOtherFolder(ProfileFiles.BottlesDirectory, "a.zip");
            theSettings.BottleFileFor("a").ShouldEqual("other".AppendPath("deployment", ProfileFiles.BottlesDirectory,"a.zip"));  

        }

        [Test]
        public void use_the_deployers_directory_if_it_is_there_but_not_in_bottles()
        {
            fileExistsAtFolder(ProfileFiles.DeployersDirectory, "a.zip");
            theSettings.BottleFileFor("a").ShouldEqual(theSettings.DeployersDirectory.AppendPath("a.zip"));  

        }

        [Test]
        public void use_the_deployers_directory_in_other_if_it_can_be_found()
        {
            fileExistsAtOtherFolder(ProfileFiles.DeployersDirectory, "a.zip");
            theSettings.BottleFileFor("a").ShouldEqual("other".AppendPath("deployment", ProfileFiles.DeployersDirectory, "a.zip"));

        }
    }
}