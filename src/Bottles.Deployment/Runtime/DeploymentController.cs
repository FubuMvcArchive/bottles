using System.Collections.Generic;
using Bottles.Deployment.Diagnostics;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using System.Linq;
using FubuCore;

namespace Bottles.Deployment.Runtime
{
    public class DeploymentController : IDeploymentController
    {
        private readonly IBottleRepository _bottles;
        private readonly IFileSystem _system;
        private readonly IDirectiveRunnerFactory _factory;
        private readonly IProfileReader _reader;
        private readonly IDiagnosticsReporter _reporter;

        public DeploymentController(IProfileReader reader, IDiagnosticsReporter reporter, IDirectiveRunnerFactory factory, IBottleRepository bottles, IFileSystem system)
        {
            _reader = reader;
            _reporter = reporter;
            _factory = factory;
            _bottles = bottles;
            _system = system;
        }

        // TODO -- end to end tests on this monster -- including failure cases
        public void Deploy(DeploymentOptions options)
        {
            // need to log inside of reader
            var plan = BuildPlan(options);
            
            try
            {
                var runners = _factory.BuildRunners(plan);

                int totalCount = runners.Sum(x => x.InitializerCount + x.DeployerCount + x.FinalizerCount);
                LogWriter.StartSteps(totalCount, "Running all directives");

                runners.Each(x => x.InitializeDeployment());
                runners.Each(x => x.Deploy());
                runners.Each(x => x.FinalizeDeployment());

                _reporter.WriteReport(options, plan);
            }
            finally
            {
                _system.DeleteDirectory(plan.Settings.StagingDirectory);
            }

            
        }

        public DeploymentPlan BuildPlan(DeploymentOptions options)
        {
            var plan = _reader.Read(options);

            plan.WriteToConsole();

            _bottles.AssertAllBottlesExist(plan.BottleNames());
            return plan;
        }
    }
}