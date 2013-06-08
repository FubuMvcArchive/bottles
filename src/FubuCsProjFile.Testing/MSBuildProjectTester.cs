using FubuCore;
using NUnit.Framework;

namespace FubuCsProjFile.Testing
{
    [TestFixture]
    public class MSBuildProjectTester
    {
        [Test]
        public void smoke_test_creating_a_new_MSBuild_project()
        {
            var fileSystem = new FileSystem();
            fileSystem.DeleteDirectory("myfoo");
            fileSystem.CreateDirectory("myfoo");

            var project = MSBuildProject.Create("MyFoo");
            project.Save("myfoo".AppendPath("MyFoo.csproj"));
        }
    }
}