using System;
using System.IO;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Tests
{
    [TestFixture]
    public class FileSystemTester : InteractionContext<FileSystem>
    {
        protected override void beforeEach()
        {
            clearDir(@"home");

            Directory.CreateDirectory(@"home");

            File.Create("home".AppendPath("bob.txt")).Close();
            File.Create("home".AppendPath("mary.txt")).Close();
        }

        void clearFile(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        void clearDir(string dir)
        {
            if(Directory.Exists(dir))
                Directory.Delete(dir,true);
        }

        [Test]
        public void should_work_with_relative_paths()
        {
            clearDir(@"rome");

            ClassUnderTest.Copy("home", "rome");

            ClassUnderTest.DirectoryExists("rome").ShouldBeTrue();
            ClassUnderTest.FileExists("rome".AppendPath("bob.txt")).ShouldBeTrue();
            ClassUnderTest.FileExists("rome".AppendPath("mary.txt")).ShouldBeTrue();

            clearDir(@"rome");
        }

        [Test]
        public void should_copy_file_to_dir()
        {
            clearDir(@"italy");

            Directory.CreateDirectory(@"italy");

            ClassUnderTest.Copy("home".AppendPath("bob.txt"), @"italy");

            ClassUnderTest.FileExists("italy".AppendPath("bob.txt")).ShouldBeTrue();
        }

        [Test]
        public void should_copy_to_non_existent_dirs()
        {
            clearDir(@"atlantis");

            ClassUnderTest.Copy("home".AppendPath("bob.txt"), @"atlantis\");

            ClassUnderTest.FileExists("atlantis".AppendPath("bob.txt")).ShouldBeTrue();

            clearDir(@"atlantis");
        }

        [Test]
        public void should_copy_as_a_file()
        {
            clearFile(@"atlantis");

            ClassUnderTest.Copy("home".AppendPath("bob.txt"), "atlantis");

            ClassUnderTest.FileExists("atlantis").ShouldBeTrue();

            clearFile("atlantis");
        }

        
    }
}