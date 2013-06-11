using System;
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
        public void read_the_project_guid()
        {
            var project = CsProjFile.LoadFrom("FubuMVC.SlickGrid.Docs.csproj");
            project.ProjectGuid.ShouldEqual(Guid.Parse("CACA4EC1-7F9A-4E38-A0A4-94FB4E23B91C"));
        }

        [Test]
        public void read_the_project_name()
        {
            var project = CsProjFile.LoadFrom("FubuMVC.SlickGrid.Docs.csproj");
            project.ProjectName.ShouldEqual("FubuMVC.SlickGrid.Docs");
        }

        [Test]
        public void read_the_project_types_when_it_is_explicit_in_the_project()
        {
            var project = CsProjFile.LoadFrom("SlickGridHarness.csproj");
            project.ProjectTypes().ShouldHaveTheSameElementsAs(Guid.Parse("349c5851-65df-11da-9384-00065b846f21"), Guid.Parse("fae04ec0-301f-11d3-bf4b-00c04f79efbc"));
        }

        [Test]
        public void read_project_type_as_a_class_library_if_no_explicit_project_type()
        {
            var project = CsProjFile.LoadFrom("FubuMVC.SlickGrid.Docs.csproj");
            project.ProjectTypes().Single()
                   .ShouldEqual(Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"));
        }

        [Test]
        public void add_code_files()
        {
            fileSystem.WriteStringToFile("myproj".AppendPath("foo.cs"), "using System.Web;");
            fileSystem.WriteStringToFile("myproj".AppendPath("bar.cs"), "using System.Web;");

            var project = CsProjFile.CreateAtSolutionDirectory("MyProj", "myproj");
            project.Add<CodeFile>("foo.cs");
            project.Add<CodeFile>("bar.cs");

            project.Save();

            var project2 = CsProjFile.LoadFrom(project.FileName);
            project2.All<CodeFile>().Select(x => x.Include)
                .ShouldHaveTheSameElementsAs("bar.cs", "foo.cs");

            project2.Add<CodeFile>("ten.cs");
            project2.Add<CodeFile>("aaa.cs");

            project2.Save();

            var project3 = CsProjFile.LoadFrom(project2.FileName);
            project3.All<CodeFile>().Select(x => x.Include)
                .ShouldHaveTheSameElementsAs("aaa.cs", "bar.cs", "foo.cs", "ten.cs");


        }

        [Test]
        public void adding_items_is_idempotent()
        {
            fileSystem.WriteStringToFile("myproj".AppendPath("foo.cs"), "using System.Web;");
            fileSystem.WriteStringToFile("myproj".AppendPath("bar.cs"), "using System.Web;");

            var project = CsProjFile.CreateAtSolutionDirectory("MyProj", "myproj");
            project.Add<CodeFile>("foo.cs");
            project.Add<CodeFile>("bar.cs");

            project.Add<CodeFile>("foo.cs");
            project.Add<CodeFile>("bar.cs");


            project.Add<CodeFile>("bar.cs");
            project.Add<CodeFile>("foo.cs");

            project.All<CodeFile>().Select(x => x.Include).ShouldHaveTheSameElementsAs("bar.cs", "foo.cs");

        }

        [Test]
        public void can_read_embedded_resources()
        {
            var project = CsProjFile.LoadFrom("FubuMVC.SlickGrid.Docs.csproj");
            project.Save();

            project = CsProjFile.LoadFrom(project.FileName);

            project.All<EmbeddedResource>().Select(x => x.Include)
                .ShouldHaveTheSameElementsAs("pak-Config.zip", "pak-Data.zip", "pak-WebContent.zip");
        }

        [Test]
        public void can_write_embedded_resources()
        {
            fileSystem.WriteStringToFile("myproj".AppendPath("foo.txt"), "using System.Web;");
            fileSystem.WriteStringToFile("myproj".AppendPath("bar.txt"), "using System.Web;");

            var project = CsProjFile.CreateAtSolutionDirectory("MyProj", "myproj");
            project.Add<EmbeddedResource>("foo.txt");
            project.Add<EmbeddedResource>("bar.txt");

            project.Save();

            var project2 = CsProjFile.LoadFrom(project.FileName);
            project2.All<EmbeddedResource>().Select(x => x.Include)
                .ShouldHaveTheSameElementsAs("bar.txt", "foo.txt");

            project2.Add<EmbeddedResource>("ten.txt");
            project2.Add<EmbeddedResource>("aaa.txt");

            project2.Save();

            var project3 = CsProjFile.LoadFrom(project2.FileName);
            project3.All<EmbeddedResource>().Select(x => x.Include)
                .ShouldHaveTheSameElementsAs("aaa.txt", "bar.txt", "foo.txt", "ten.txt");


        }
    }
}