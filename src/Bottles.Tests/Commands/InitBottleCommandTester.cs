using Bottles.Commands;
using FubuCore;
using NUnit.Framework;
using FubuTestingSupport;

namespace Bottles.Tests.Commands
{
    [TestFixture]
    public class InitBottleCommandTester 
    {
        IFileSystem fs = new FileSystem();
        private string thePath = "initpath";
        private string pakName = "init-test";
        private InitInput theInput;

        [SetUp]
        public void BeforeEach()
        {
            fs.DeleteDirectory(thePath);
            theInput = new InitInput
                       {
                           Name = pakName,
                           Path = thePath
                       };
        }

        string checkForAlias(string alias)
        {
            var a = new FileSystem()
                  .LoadFromFile<AliasRegistry>(AliasRegistry.ALIAS_FILE);

            return a.AliasFor(alias).Folder;
        }

        void execute()
        {
            
            var cmd = new InitCommand();
            cmd.Execute(theInput);
        }

        [Test]
        public void the_pak_should_have_been_created()
        {
            execute();

            fs.FileExists(thePath, PackageManifest.FILE).ShouldBeTrue();

            checkForAlias(pakName).ShouldEqual(thePath);

            var pm = fs.LoadPackageManifestFrom(thePath);
            pm.Name.ShouldEqual(pakName);
        }

        [Test]
        public void the_pak_should_have_been_created_with_alias()
        {
            var theAlias = "blue";
            theInput.AliasFlag = theAlias;

            execute();

            fs.FileExists(thePath, PackageManifest.FILE).ShouldBeTrue();

            checkForAlias(theAlias).ShouldEqual(thePath);
        }

        [Test]
        public void the_pak_should_not_be_overridden_if_already_exists()
        {
            execute();
            fs.FileExists(thePath, PackageManifest.FILE).ShouldBeTrue();

            var pm = fs.LoadPackageManifestFrom(thePath);
            pm.Name.ShouldEqual(pakName);

            theInput.Name = "NewName";

            execute();

            pm = fs.LoadPackageManifestFrom(thePath);
            pm.Name.ShouldEqual(pakName);
        }

        [Test]
        public void the_existing_pak_should_be_overridden_if_force_flag()
        {
            execute();
            fs.FileExists(thePath, PackageManifest.FILE).ShouldBeTrue();

            var pm = fs.LoadPackageManifestFrom(thePath);
            pm.Name.ShouldEqual(pakName);

            theInput.Name = "NewName";
            theInput.ForceFlag = true;

            execute();

            pm = fs.LoadPackageManifestFrom(thePath);
            pm.Name.ShouldEqual("NewName");
        }


        [Test]
        public void the_pak_should_have_env_stuff_set()
        {
            execute();

            var pm = fs.LoadPackageManifestFrom(thePath);

            pm.Name.ShouldEqual(pakName);

            pm.Role.ShouldEqual(BottleRoles.Module);
        }

        [Test]
        public void the_pak_should_have_role_overrided()
        {
            theInput.RoleFlag = BottleRoles.Binaries;

            execute();

            var pm = fs.LoadPackageManifestFrom(thePath);

            pm.Role.ShouldEqual(BottleRoles.Binaries);
        }
    }
}