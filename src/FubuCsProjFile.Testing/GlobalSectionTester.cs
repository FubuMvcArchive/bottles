using FubuTestingSupport;
using NUnit.Framework;

namespace FubuCsProjFile.Testing
{
    [TestFixture]
    public class GlobalSectionTester
    {
        private GlobalSection theSection;

        [SetUp]
        public void SetUp()
        {
            theSection = new GlobalSection("    GlobalSection(SolutionProperties) = preSolution");
        }

        [Test]
        public void the_declaration()
        {
            theSection.Declaration.ShouldEqual("GlobalSection(SolutionProperties) = preSolution");

        }

        [Test]
        public void name()
        {
            theSection.SectionName.ShouldEqual("SolutionProperties");
        }

        [Test]
        public void order()
        {
            theSection.LoadingOrder.ShouldEqual(SolutionLoading.preSolution);
        }
    }
}