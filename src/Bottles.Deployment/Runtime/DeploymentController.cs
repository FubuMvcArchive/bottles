using System;
using System.Collections.Generic;
using System.IO;
using Bottles.Deployment.Diagnostics;
using Bottles.Deployment.Parsing;

namespace Bottles.Deployment.Runtime
{
    public class DeploymentController : IDeploymentController
    {
        private readonly IProfileReader _reader;
        private readonly IDiagnosticsReporter _reporter;
        private readonly IDirectiveRunnerFactory _factory;

        public DeploymentController(IProfileReader reader, IDiagnosticsReporter reporter, IDirectiveRunnerFactory factory)
        {
            _reader = reader;
            _reporter = reporter;
            _factory = factory;
        }

        public void Deploy(DeploymentOptions options)
        {
            // need to log inside of reader
            var plan = _reader.Read(options);

            var runners = _factory.BuildRunners(plan);

            runners.Each(x => x.InitializeDeployment());
            runners.Each(x => x.Deploy());
            runners.Each(x => x.FinalizeDeployment());

           _reporter.WriteReport(options, plan);
        }

    }
}