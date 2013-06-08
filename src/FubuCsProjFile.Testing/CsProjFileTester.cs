using FubuCore;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;

namespace FubuCsProjFile.Testing
{
    [TestFixture]
    public class CsProjFileTester
    {
        private FileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new FileSystem();
            fileSystem.DeleteDirectory("myproj");
            fileSystem.CreateDirectory("myproj");
        }

        [Test]
        public void add_code_files()
        {
            fileSystem.WriteStringToFile("myproj".AppendPath("foo.cs"), "using System.Web;");
            fileSystem.WriteStringToFile("myproj".AppendPath("bar.cs"), "using System.Web;");

            var project = CsProjFile.CreateAtSolutionDirectory("MyProj", "myproj");
            project.AddCodeFile("foo.cs");
            project.AddCodeFile("bar.cs");

            project.Save();

            var project2 = CsProjFile.LoadFrom(project.FileName);
            project2.CodeFiles().Select(x => x.RelativePath)
                .ShouldHaveTheSameElementsAs("bar.cs", "foo.cs");

            project2.AddCodeFile("ten.cs");
            project2.AddCodeFile("aaa.cs");

            project2.Save();

            var project3 = CsProjFile.LoadFrom(project2.FileName);
            project3.CodeFiles().Select(x => x.RelativePath)
                .ShouldHaveTheSameElementsAs("aaa.cs", "bar.cs", "foo.cs", "ten.cs");


        }
    }
}