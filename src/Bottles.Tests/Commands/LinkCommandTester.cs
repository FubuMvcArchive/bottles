using System.IO;
using Bottles.Commands;
using Bottles.Deployment.Commands;
using Bottles.PackageLoaders.LinkedFolders;
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
        private LinkManifest btlManifest;

        protected override void beforeEach()
        {
            theInput = new LinkInput
            {
                BottleFolder = "package",
                AppFolder = "app",
            };

            appManifest = new LinkManifest();
            btlManifest = new LinkManifest();
        }

        private void theManifestFileExists()
        {
            MockFor<ILinksService>().Stub(x => x.LinkManifestExists(theInput.BottleFolder)).Return(true);
            MockFor<ILinksService>().Stub(x => x.LinkManifestExists(theInput.AppFolder)).Return(true);

            MockFor<ILinksService>().Stub(x => x.GetLinkManifest(theInput.BottleFolder)).Return(btlManifest);
            MockFor<ILinksService>().Stub(x => x.GetLinkManifest(theInput.AppFolder)).Return(appManifest);
        }


        private void execute()
        {
            ClassUnderTest.Execute(theInput, MockFor<ILinksService>());
        }

        [Test]
        public void should_link_app_to_package()
        {
            var expectedFolder = "..".AppendPath(theInput.BottleFolder);
            theManifestFileExists();

            execute();

            appManifest.LinkedFolders.ShouldContain(expectedFolder);
        }

        [Test]
        public void should_link_app_to_package_with_trailing_slash_for_app()
        {
            var expectedFolder = "..".AppendPath(theInput.BottleFolder);
            theInput.AppFolder += Path.DirectorySeparatorChar;
            theManifestFileExists();

            execute();

            appManifest.LinkedFolders.ShouldContain(expectedFolder);
        }
    }
}