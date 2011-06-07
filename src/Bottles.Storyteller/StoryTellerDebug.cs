using System;
using System.IO;
using FubuCore;
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
            var root = Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory);
            runner = new ProjectTestRunner(root.AppendPath("git", "bottles", "src", "Bottles.Storyteller", "BottlesStoryTeller.xml"));
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