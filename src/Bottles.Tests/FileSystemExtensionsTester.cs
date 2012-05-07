using System.Diagnostics;
using Bottles.Creation;
using FubuCore;
using NUnit.Framework;
using Rhino.Mocks;
using FubuTestingSupport;

namespace Bottles.Tests
{
    [TestFixture]
    public class FileSystemExtensionsTester
    {
        private IFileSystem theFileSystem;
        private string theFolder = "directory2";

        [SetUp]
        public void SetUp()
        {
            theFileSystem = MockRepository.GenerateMock<IFileSystem>();
        }

        [Test]
        public void find_binary_directory_if_the_target_directory_does_not_exist()
        {
            theFileSystem
                .Stub(x => x.DirectoryExists(theFolder, "bin", CompileTarget.Debug.ToString()))
                .Return(false);

            theFileSystem.FindBinaryDirectory(theFolder, CompileTarget.Debug.ToString())
                .ShouldEqual(FileSystem.Combine(theFolder, "bin"));


        }

        [Test]
        public void find_binary_directory_when_the_target_directory_exists()
        {
            theFileSystem
                .Stub(x => x.DirectoryExists(theFolder, "bin", CompileTarget.Release.ToString()))
                .Return(true);

            theFileSystem.FindBinaryDirectory(theFolder, "Release")
                .ShouldEqual(FileSystem.Combine(theFolder, "bin", CompileTarget.Release.ToString())); 
        }

        [Test]
        public void find_assembly_names_smoke_tester()
        {
            var names = new FileSystem().FindAssemblyNames(".".ToFullPath());
        
            names.ShouldContain("StructureMap");
            names.ShouldContain("Bottles");
            names.ShouldContain("Bottles.Tests");
            names.ShouldContain("FubuCore");
        }
    }
}   