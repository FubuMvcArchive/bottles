using FubuCore;
using FubuCsProjFile.MSBuild;
using NUnit.Framework;

namespace FubuCsProjFile.Testing
{
    [TestFixture]
    public class MSBuildProjectTester
    {
        [SetUp]
        public void SetUp()
        {
            var fileSystem = new FileSystem();
            fileSystem.DeleteDirectory("myfoo");
            fileSystem.CreateDirectory("myfoo");
        }

        [Test]
        public void smoke_test_creating_a_new_MSBuild_project()
        {
            var project = MSBuildProject.Create("MyFoo");
            var fileName = "myfoo".AppendPath("MyFoo.csproj");
            project.Save(fileName);

            new MSBuildProject().Load(fileName);
        }
    }
}