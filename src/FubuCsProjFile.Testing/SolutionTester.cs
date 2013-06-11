using System.Diagnostics;
using FubuCore;
using NUnit.Framework;
using FubuTestingSupport;
using System.Collections.Generic;

namespace FubuCsProjFile.Testing
{
    [TestFixture]
    public class SolutionTester
    {
        [Test]
        public void create_new_and_read_preamble()
        {
            var solution = Solution.CreateNew(".", "foo");
            solution.Preamble.ShouldHaveTheSameElementsAs("Microsoft Visual Studio Solution File, Format Version 11.00", "# Visual Studio 2010");
        }

        [Test]
        public void create_new_and_read_build_configurations()
        {
            var solution = Solution.CreateNew(".", "foo");
            solution.Configurations().ShouldHaveTheSameElementsAs(
                new BuildConfiguration("Debug|Any CPU = Debug|Any CPU"),
                new BuildConfiguration("Debug|Mixed Platforms = Debug|Mixed Platforms"),
                new BuildConfiguration("Debug|x86 = Debug|x86"),
                new BuildConfiguration("Release|Any CPU = Release|Any CPU"),
                new BuildConfiguration("Release|Mixed Platforms = Release|Mixed Platforms"),
                new BuildConfiguration("Release|x86 = Release|x86")
                
                
                
                );
        }

        [Test]
        public void create_new_and_read_other_globals()
        {
            var solution = Solution.CreateNew(".", "foo");
            solution.FindSection("SolutionProperties").Properties
                .ShouldHaveTheSameElementsAs("HideSolutionNode = FALSE");
        }

        [Test]
        public void write_a_solution()
        {
            var solution = Solution.CreateNew(".".ToFullPath(), "foo");
            solution.Save();

            var original =
                new FileSystem().ReadStringFromFile(
                    ".".ToFullPath()
                       .ParentDirectory()
                       .ParentDirectory()
                       .ParentDirectory()
                       .AppendPath("FubuCsProjFile", "Solution.txt")).SplitOnNewLine();

            var newContent = new FileSystem().ReadStringFromFile("foo.sln").SplitOnNewLine();

            newContent.ShouldHaveTheSameElementsAs(original);

        }
    }
}