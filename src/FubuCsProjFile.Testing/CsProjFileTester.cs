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

        [Test]
        public void can_read_embedded_resources()
        {
            var project = CsProjFile.LoadFrom("FubuMVC.SlickGrid.Docs.csproj");
            project.Save();

            project = CsProjFile.LoadFrom(project.FileName);

            project.EmbeddedResources().Select(x => x.RelativePath)
                .ShouldHaveTheSameElementsAs("pak-Config.zip", "pak-Data.zip", "pak-WebContent.zip");
        }

        [Test]
        public void can_write_embedded_resources()
        {
            fileSystem.WriteStringToFile("myproj".AppendPath("foo.txt"), "using System.Web;");
            fileSystem.WriteStringToFile("myproj".AppendPath("bar.txt"), "using System.Web;");

            var project = CsProjFile.CreateAtSolutionDirectory("MyProj", "myproj");
            project.AddEmbeddedResource("foo.txt");
            project.AddEmbeddedResource("bar.txt");

            project.Save();

            var project2 = CsProjFile.LoadFrom(project.FileName);
            project2.EmbeddedResources().Select(x => x.RelativePath)
                .ShouldHaveTheSameElementsAs("bar.txt", "foo.txt");

            project2.AddEmbeddedResource("ten.txt");
            project2.AddEmbeddedResource("aaa.txt");

            project2.Save();

            var project3 = CsProjFile.LoadFrom(project2.FileName);
            project3.EmbeddedResources().Select(x => x.RelativePath)
                .ShouldHaveTheSameElementsAs("aaa.txt", "bar.txt", "foo.txt", "ten.txt");


        }
    }
}