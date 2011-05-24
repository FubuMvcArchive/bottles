using System.IO;
using Bottles.Commands;
using Bottles.Deployment.Commands;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Bottles.Tests.Commands
{
    [TestFixture]
    public class LinkCommandTester : InteractionContext<LinkCommand>
    {
        private LinkInput theInput;
        private LinkManifest appManifest;
        private LinkManifest pakManifest;

        protected override void beforeEach()
        {
            theInput = new LinkInput
            {
                PackageFolder = "package",
                AppFolder = "app",
            };

            appManifest = new LinkManifest();
            pakManifest = new LinkManifest();
            Services.PartialMockTheClassUnderTest();
        }

        private void theManifestFileExists()
        {
            var LinkManifestFileName = FileSystem.Combine(theInput.PackageFolder, LinkManifest.FILE);
            MockFor<IFileSystem>().Stub(x => x.FileExists(LinkManifestFileName)).Return(true);

            MockFor<IFileSystem>().Stub(x => x.LinkManifestExists(theInput.PackageFolder)).Return(true);
            MockFor<IFileSystem>().Stub(x => x.LoadFromFile<LinkManifest>(LinkManifestFileName)).Return(pakManifest);
            MockFor<IFileSystem>().Stub(x => x.FileExists(theInput.AppFolder, LinkManifest.FILE)).Return(true);
            MockFor<IFileSystem>().Stub(x => x.LoadFromFile<LinkManifest>(theInput.AppFolder, LinkManifest.FILE)).Return(appManifest);
        }

        private string oneFolderUp(string path)
        {
            return "..{0}{1}".ToFormat(Path.DirectorySeparatorChar, path);
        }

        private void execute()
        {
            ClassUnderTest.Execute(theInput, MockFor<IFileSystem>());
        }

        [Test]
        public void should_link_app_to_package()
        {
            var expectedFolder = oneFolderUp(theInput.PackageFolder);
            theManifestFileExists();

            execute();

            appManifest.LinkedFolders.ShouldContain(expectedFolder);
        }

        [Test]
        public void should_link_app_to_package_with_trailing_slash_for_app()
        {
            var expectedFolder = oneFolderUp(theInput.PackageFolder);
            theInput.AppFolder += Path.DirectorySeparatorChar;
            theManifestFileExists();

            execute();

            appManifest.LinkedFolders.ShouldContain(expectedFolder);
        }
    }
}