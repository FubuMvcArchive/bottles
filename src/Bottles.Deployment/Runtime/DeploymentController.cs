using System;
using System.Collections.Generic;
using System.IO;
using Bottles.Deployment.Diagnostics;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime.Content;
using FubuCore.CommandLine;
using FubuCore;
using System.Linq;

namespace Bottles.Deployment.Runtime
{
    public class DeploymentController : IDeploymentController
    {
        private readonly IProfileReader _reader;
        private readonly IDiagnosticsReporter _reporter;
        private readonly IDirectiveRunnerFactory _factory;
        private readonly IBottleRepository _bottles;

        public DeploymentController(IProfileReader reader, IDiagnosticsReporter reporter, IDirectiveRunnerFactory factory, IBottleRepository bottles)
        {
            _reader = reader;
            _reporter = reporter;
            _factory = factory;
            _bottles = bottles;
        }

        public void Deploy(DeploymentOptions options)
        {
            // need to log inside of reader
            var plan = _reader.Read(options);

            writeOptionsToConsole(plan);

            _bottles.AssertAllBottlesExist(plan.BottleNames());

            var runners = _factory.BuildRunners(plan);

            runners.Each(x => x.InitializeDeployment());
            runners.Each(x => x.Deploy());
            runners.Each(x => x.FinalizeDeployment());

           _reporter.WriteReport(options, plan);
        }

        private static void writeOptionsToConsole(DeploymentPlan plan)
        {
            ConsoleWriter.Line();
            ConsoleWriter.PrintHorizontalLine();
            ConsoleWriter.Write("Deploying profile {0}".ToFormat(plan.Options.ProfileName));
            ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, 2, "from deployment directory " + plan.Settings.DeploymentDirectory);
            ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, 2, "to target directory " + plan.Settings.TargetDirectory);
            ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, 2, "Applying recipe(s):  " + plan.Recipes.Select(x => x.Name).Join(", "));
            ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, 2, "Deploying host(s):  " + plan.Hosts.Select(x => x.Name).Join(", "));
            ConsoleWriter.PrintHorizontalLine();
        }
    }
}