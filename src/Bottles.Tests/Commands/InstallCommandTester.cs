using System;
using System.IO;
using Bottles.Deployment.Commands;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Bottles.Tests.Commands
{
    [TestFixture]
    public class InstallCommandTester : InteractionContext<InstallCommand>
    {
        private InstallInput theInput;
        private PackageManifest theManifest;

        protected override void beforeEach()
        {
            theInput = new InstallInput()
            {
                AppFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "folder1")
            };

            theManifest = new PackageManifest();
            Services.PartialMockTheClassUnderTest();
        }


        private void theManifestFileDoesNotExist()
        {
            MockFor<IFileSystem>().Stub(x => FubuCore.FileSystemExtensions.FileExists(x, theInput.AppFolder, PackageManifest.FILE)).Return(false);
        }

        private void theManifestFileExists()
        {
            MockFor<IFileSystem>().Stub(x => FubuCore.FileSystemExtensions.FileExists(x, theInput.AppFolder, PackageManifest.FILE)).Return(true);
            MockFor<IFileSystem>().Stub(x => FubuCore.FileSystemExtensions.LoadFromFile<PackageManifest>(x, theInput.AppFolder, PackageManifest.FILE)).Return(theManifest);
        }


        private void execute()
        {
            ClassUnderTest.Execute(theInput, MockFor<IFileSystem>());
        }


        [Test]
        public void create_environment_run_uses_web_config_by_default_if_it_is_not_specified_in_the_input()
        {
            var run = InstallCommand.CreateEnvironmentRun(theInput);

            run.ConfigurationFile.ShouldEqual(Path.Combine(theInput.AppFolder, "web.config"));
        }

        [Test]
        public void create_environment_run_uses_the_specific_config_file_if_one_is_given()
        {
            theInput.ConfigFileFlag = "different.config";

            var run = InstallCommand.CreateEnvironmentRun(theInput);

            run.ConfigurationFile.ShouldEqual(Path.Combine(theInput.AppFolder, "different.config"));
        }

        [Test]
        public void create_environment_sets_all_the_assembly_and_class_name_properties()
        {
            theInput.EnvironmentClassNameFlag = "some class";
            theInput.EnvironmentAssemblyFlag = "some assembly";

            var run = InstallCommand.CreateEnvironmentRun(theInput);

            run.EnvironmentClassName.ShouldEqual(theInput.EnvironmentClassNameFlag);
            run.AssemblyName.ShouldEqual(theInput.EnvironmentAssemblyFlag);
        }

        [Test]
        public void create_environment_defaults_the_application_base_to_the_bin_directory_underneath_the_app_folder()
        {		
            InstallCommand.CreateEnvironmentRun(theInput).ApplicationBase
				.ShouldEqual(Path.Combine(theInput.AppFolder, "bin"));
        }

        [Test]
        public void create_environment_run_uses_the_class_name_from_the_input_if_it_exists()
        {
            theInput.EnvironmentClassNameFlag = "some class";

            InstallCommand.CreateEnvironmentRun(theInput)
                .EnvironmentClassName.ShouldEqual("some class");
        }

        [Test]
        public void create_environment_run_uses_the_config_name_from_the_input()
        {

            InstallCommand.CreateEnvironmentRun(theInput)
                .ConfigurationFile.ShouldEqual(theInput.AppFolder.AppendPath(theInput.ConfigFileFlag).ToFullPath());
        }

        [Test]
        public void create_environment_run_uses_the_assembly_from_the_input_if_it_exists()
        {
            theInput.EnvironmentAssemblyFlag = "some assembly";

            InstallCommand.CreateEnvironmentRun(theInput)
                .AssemblyName.ShouldEqual("some assembly");
        }


    }
}