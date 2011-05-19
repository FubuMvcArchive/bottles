using NUnit.Framework;
using StoryTeller.Execution;

namespace StoryTellerTestHarness
{
    [TestFixture, Explicit]
    public class Template
    {
        private ProjectTestRunner runner;

        [TestFixtureSetUp]
        public void SetupRunner()
        {
            runner = new ProjectTestRunner(@"C:\git\bottles\src\Bottles.Storyteller\BottlesStoryTeller.xml");
        }

        [Test]
        public void Environment_and_Profile_substitutions()
        {
            runner.RunAndAssertTest("Reading/Environment and Profile substitutions");
        }

        [TestFixtureTearDown]
        public void TeardownRunner()
        {
            runner.Dispose();
        }
    }
}