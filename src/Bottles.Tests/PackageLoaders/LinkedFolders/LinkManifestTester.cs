using System.Linq;
using Bottles.Commands;
using Bottles.PackageLoaders.LinkedFolders;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Tests.PackageLoaders.LinkedFolders
{
    [TestFixture]
    public class LinkManifestTester
    {
        [Test]
        public void remove_all_linked_folders_gets_remotes_too()
        {
            var manifest = new LinkManifest();
            manifest.AddLink("foo");
            manifest.AddRemoteLink(new LinkInput
            {
                BottleFolder = "foo"
            });

            manifest.RemoteLinks.Any().ShouldBeTrue();
            manifest.LinkedFolders.Any().ShouldBeTrue();

            manifest.RemoveAllLinkedFolders();

            manifest.RemoteLinks.Any().ShouldBeFalse();
            manifest.LinkedFolders.Any().ShouldBeFalse();

        }

        [Test]
        public void add_remote_link_modifies_existing()
        {
            var manifest = new LinkManifest();
            manifest.AddRemoteLink(new LinkInput
            {
                BottleFolder = "foo"
            });

            manifest.AddRemoteLink(new LinkInput
            {
                BottleFolder = "foo",
                BootstrapperFlag = "SomeClass"
            });

            manifest.AddRemoteLink(new LinkInput
            {
                BottleFolder = "foo",
                BootstrapperFlag = "OtherClass",
                ConfigFileFlag = "Web.config"
            });

            var link = manifest.RemoteLinks.Single();
            link.Folder.ShouldEqual("foo");
            link.BootstrapperName.ShouldEqual("OtherClass");
            link.ConfigFile.ShouldEqual("Web.config");
        }

        [Test]
        public void remove_link_can_get_remote_link_too()
        {
            var manifest = new LinkManifest();
            manifest.AddRemoteLink(new LinkInput
            {
                BottleFolder = "foo"
            });

            manifest.RemoveLink("foo");

            manifest.RemoteLinks.Any().ShouldBeFalse();
        }
    }
}