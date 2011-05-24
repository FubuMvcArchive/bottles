using System;
using Bottles.Deployment;
using Bottles.Deployment.Deployers.Configuration;
using Bottles.Diagnostics;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;
using FubuCore;

namespace Bottles.Tests.Configuration
{
    [TestFixture]
    public class ResolveDbConnectionDeployerTester : InteractionContext<ResolveDbConnectionDeployer>
    {
        private HostManifest theHost;
        private IDirectiveWithRoot thePathedDirective;
        private ResolveDbConnection theDirective;

        protected override void beforeEach()
        {
            theHost = MockRepository.GenerateMock<HostManifest>("something");
            thePathedDirective = MockRepository.GenerateMock<IDirectiveWithRoot>();
            thePathedDirective.Stub(x => x.ApplicationRootDirectory()).Return("folder1");


            theDirective = new ResolveDbConnection()
            {
                File = "connections.config"
            };
        }


        [Test]
        public void execute_with_no_path()
        {
            theHost.Stub(x => x.Directives).Return(new IDirective[0]);

            ClassUnderTest.Execute(theDirective, theHost, MockFor<IPackageLog>());

            MockFor<IConnectionStringResolver>().AssertWasNotCalled(x => x.Resolve(null), x => x.IgnoreArguments());
            MockFor<IPackageLog>().AssertWasCalled(x => x.MarkFailure(ResolveDbConnectionDeployer.COULD_NOT_FIND_DIRECTIVE_PATH));
        }

        [Test]
        public void execute_with_path()
        {
            theHost.Stub(x => x.Directives).Return(new IDirective[]{thePathedDirective});

            ClassUnderTest.Execute(theDirective, theHost, MockFor<IPackageLog>());

            MockFor<IConnectionStringResolver>().AssertWasCalled(x => x.Resolve("folder1".AppendPath(theDirective.File).ToFullPath()));
        }
    }
}