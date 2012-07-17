using System.Collections.Generic;
using System.IO;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Tests
{
    [TestFixture]
    public class BottleInfoTester
    {
        private BottleInfo _theBottle;
		private string theDataFolder;

        [SetUp]
        public void SetUp()
        {
			theDataFolder = "data";
			
            if (Directory.Exists(theDataFolder))
            {
                Directory.Delete(theDataFolder, true);
            }

            _theBottle = new BottleInfo(new PackageManifest(){Name="a"});
            _theBottle.RegisterFolder(WellKnownFiles.DataFolder, Path.GetFullPath(theDataFolder));
        }

        [Test]
        public void add_dependency_returns_later()
        {
            var dep1 = Dependency.Optional("Bottle1");
            var dep2 = Dependency.Optional("Bottle2");
            var dep3 = Dependency.Optional("Bottle3");
        
            _theBottle.AddDependency(dep1);
            _theBottle.AddDependency(dep2);
            _theBottle.AddDependency(dep3);

            _theBottle.As<IBottleInfo>().Dependencies
                .ShouldHaveTheSameElementsAs(dep1, dep2, dep3);
        }

        private string join(params string[] paths)
        {
            return paths.Join(Path.DirectorySeparatorChar.ToString());
        }

        private void writeText(string name, string text)
        {
            var directory = Path.GetDirectoryName(name);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(name))
            {
                File.Delete(name);
            }

            File.WriteAllText(name, text);
        }
        
        public IEnumerable<string> readFiles(string searchString)
        {
            var list = new List<string>();
            ((IBottleInfo)_theBottle).ForFiles(WellKnownFiles.DataFolder, searchString, (name, stream) => list.Add(stream.ReadAllText()));

            list.Sort();

            return list;
        }

        [Test]
        public void happily_do_nothing_if_caller_requests_a_folder_That_does_not_exist()
        {
            ((IBottleInfo)_theBottle).ForFiles(WellKnownFiles.DataFolder, join("nonexistent", "*.xml"), (x, y) => Assert.Fail("Not supposed to call this"));
        }

        [Test]
        public void get_the_file_names()
        {
            writeText(FileSystem.Combine(theDataFolder, "st", "a.txt"), "a");
            writeText(FileSystem.Combine(theDataFolder, "st", "b.txt"), "b");
            writeText(FileSystem.Combine(theDataFolder, "c.txt"), "c");
            writeText(FileSystem.Combine(theDataFolder, "st", "d.t2"), "d");
            writeText(FileSystem.Combine(theDataFolder, "e.t2"), "e");

            var list = new List<string>();
            ((IBottleInfo)_theBottle).ForFiles(WellKnownFiles.DataFolder, "*.*", (name, stream) => list.Add(name));

            list.Sort();
            list.ShouldHaveTheSameElementsAs("c.txt", "e.t2", join("st", "a.txt"), join("st",  "b.txt"), join("st", "d.t2"));
        }

        [Test]
        public void read_data_with_just_the_extension()
        {
            writeText(FileSystem.Combine(theDataFolder, "a.txt"), "a");
            writeText(FileSystem.Combine(theDataFolder, "b.txt"), "b");
            writeText(FileSystem.Combine(theDataFolder, "c.txt"), "c");
            writeText(FileSystem.Combine(theDataFolder, "d.t2"), "d");
            writeText(FileSystem.Combine(theDataFolder, "e.t2"), "e");

            readFiles("*.txt").ShouldHaveTheSameElementsAs("a", "b", "c");
            readFiles("*.t2").ShouldHaveTheSameElementsAs("d", "e");
        }

        [Test]
        public void read_data_from_a_folder_and_extension()
        {
            writeText(FileSystem.Combine(theDataFolder, "st", "a.txt"), "a");
            writeText(FileSystem.Combine(theDataFolder, "st", "b.txt"), "b");
            writeText(FileSystem.Combine(theDataFolder, "c.txt"), "c");
            writeText(FileSystem.Combine(theDataFolder, "st", "d.t2"), "d");
            writeText(FileSystem.Combine(theDataFolder, "e.t2"), "e");

            readFiles("*.txt").ShouldHaveTheSameElementsAs("a", "b", "c");
            readFiles("*.t2").ShouldHaveTheSameElementsAs("d", "e");

            readFiles(join("st", "*.txt")).ShouldHaveTheSameElementsAs("a", "b");
            readFiles(join("st", "*.t2")).ShouldHaveTheSameElementsAs("d");
        }
    }
}