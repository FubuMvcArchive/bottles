using System;
using System.IO;
using Bottles.Deployment.Deployers.CommandLine;
using FubuTestingSupport;
using NUnit.Framework;
using FubuCore;

namespace Bottles.Tests.Deployment.Deployers.CommandLine
{
    [TestFixture]
    public class CommandLineDeployerTester : InteractionContext<CommandLineDeployer>
    {
        private CommandLineExecution theDirective;

        protected override void beforeEach()
        {
            theDirective = new CommandLineExecution(){
                Arguments = "dbgen baseline",
                WorkingDirectory = ".".ToFullPath()
            };
        }

        [Test]
        public void build_process_start_for_rooted_file()
        {
            theDirective.FileName = Path.GetFullPath(".").AppendPath("blue.exe");
            ClassUnderTest.GetProcessStartInfo(theDirective)
                .FileName.ShouldEqual(theDirective.FileName);
        }

        [Test]
        public void build_process_for_a_non_rooted_file()
        {
            theDirective.FileName = "blue.exe";
            ClassUnderTest.GetProcessStartInfo(theDirective)
                .FileName.ShouldEqual(theDirective.WorkingDirectory.AppendPath(theDirective.FileName));
        }

    }
}