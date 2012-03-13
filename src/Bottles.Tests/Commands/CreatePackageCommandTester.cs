using System;
using System.IO;
using Bottles.Commands;
using Bottles.Creation;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Bottles.Tests.Commands
{
    [TestFixture]
    public class CreatePackageCommandTester : InteractionContext<CreateBottleCommand>
    {
        private CreateBottleInput theInput;
        private PackageManifest theManifest;

        protected override void beforeEach()
        {
            var root = Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory);
            theInput = new CreateBottleInput()
            {
                PackageFolder = "some folder",
                ZipFileFlag = root.AppendPath("package1.zip")
            };

            theManifest = new PackageManifest();
            Services.PartialMockTheClassUnderTest();
        }


        private void theManifestFileDoesNotExist()
        {
            MockFor<IFileSystem>().Stub(x => x.PackageManifestExists(theInput.PackageFolder)).Return(false);
        }

        private void theManifestFileExists()
        {
            MockFor<IFileSystem>().Stub(x => x.FileExists(theInput.PackageFolder, PackageManifest.FILE)).Return(true);
            MockFor<IFileSystem>().Stub(x => x.PackageManifestExists(theInput.PackageFolder)).Return(true);
            MockFor<IFileSystem>().Stub(x => x.LoadFromFile<PackageManifest>(theInput.PackageFolder, PackageManifest.FILE)).Return(theManifest);
        }

        private void theZipFileAlreadyExists()
        {
            MockFor<IFileSystem>().Stub(x => x.FileExists(theInput.ZipFileFlag)).Return(true);
        }

        private void theZipFileDoesNotExist()
        {
            MockFor<IFileSystem>().Stub(x => x.FileExists(theInput.ZipFileFlag)).Return(false);
        }

        private void execute()
        {
            ClassUnderTest.Execute(theInput, MockFor<IFileSystem>());
        }

        [Test]
        public void delete_the_existing_package_zip_file_if_it_exists_and_force_is_true()
        {
            theManifestFileExists();
            theZipFileAlreadyExists();
            theInput.ForceFlag = true;

            // Just forcing this method to be self-mocked
            ClassUnderTest.Expect(x => x.CreatePackage(theInput, MockFor<IFileSystem>()));
            ClassUnderTest.Expect(x => x.WriteZipFileAlreadyExists(theInput.ZipFileFlag));

            execute();


            ClassUnderTest.AssertWasNotCalled(x => x.WriteZipFileAlreadyExists(theInput.ZipFileFlag));
            MockFor<IFileSystem>().AssertWasCalled(x => x.DeleteFile(theInput.ZipFileFlag));

        }

        [Test]
        public void do_not_delete_the_existing_zip_file_if_it_exists_and_force_flag_is_false()
        {
            theManifestFileExists();
            theZipFileAlreadyExists();
            theInput.ForceFlag = false;

            // Just forcing this method to be self-mocked
            ClassUnderTest.Expect(x => x.CreatePackage(theInput, MockFor<IFileSystem>()));
            ClassUnderTest.Expect(x => x.WriteZipFileAlreadyExists(theInput.ZipFileFlag));

            execute();

            MockFor<IFileSystem>().AssertWasNotCalled(x => x.DeleteFile(theInput.ZipFileFlag));
        
            ClassUnderTest.AssertWasNotCalled(x => x.CreatePackage(theInput, MockFor<IFileSystem>()));
            ClassUnderTest.AssertWasCalled(x => x.WriteZipFileAlreadyExists(theInput.ZipFileFlag));
        }

        [Test]
        public void create_the_package_if_the_package_manifest_file_exists()
        {
            theManifestFileExists();
            theZipFileDoesNotExist();

            // Just forcing this method to be self-mocked
            ClassUnderTest.Expect(x => x.CreatePackage(theInput, MockFor<IFileSystem>()));
            ClassUnderTest.Expect(x => x.WritePackageManifestDoesNotExist(theInput.PackageFolder)).Repeat.Never();

            execute();

            ClassUnderTest.VerifyAllExpectations();
        }

        [Test]
        public void do_not_create_the_package_if_the_package_manifest_file_does_not_exist()
        {
            theManifestFileDoesNotExist();
            theZipFileDoesNotExist();

            // Just forcing this method to be self-mocked
            ClassUnderTest.Expect(x => x.CreatePackage(theInput, MockFor<IFileSystem>())).Repeat.Never();
            ClassUnderTest.Expect(x => x.WritePackageManifestDoesNotExist(theInput.PackageFolder));

            execute();

            ClassUnderTest.VerifyAllExpectations();
        }
    }
}