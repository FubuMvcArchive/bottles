using System;
using Bottles.Deployment;
using Bottles.Diagnostics;
using NUnit.Framework;
using Rhino.Mocks;

namespace Bottles.Tests.Deployment
{
    [TestFixture]
    public class ProcessReturnTester
    {
        [Test]
        public void assert_optional_success_with_no_failures()
        {
            var procReturn = new ProcessReturn(){
                ExitCode = 0,
                OutputText = "something"
            };

            var log = MockRepository.GenerateMock<IPackageLog>();
            LogWriter.WithLog(log, () =>
            {
                procReturn.AssertOptionalSuccess();
            });

            log.AssertWasCalled(x => x.Trace(procReturn.OutputText));
        }

        [Test]
        public void assert_optional_success_with_failures_still_only_traces()
        {
            var procReturn = new ProcessReturn()
            {
                ExitCode = 11,
                OutputText = "something"
            };

            var log = MockRepository.GenerateMock<IPackageLog>();
            LogWriter.WithLog(log, () =>
            {
                procReturn.AssertOptionalSuccess();
            });

            log.AssertWasCalled(x => x.Trace(procReturn.OutputText));
        }

        [Test]
        public void assert_mandatory_success_with_no_failures()
        {
            var procReturn = new ProcessReturn()
            {
                ExitCode = 0,
                OutputText = "something"
            };

            var log = MockRepository.GenerateMock<IPackageLog>();
            LogWriter.WithLog(log, () =>
            {
                procReturn.AssertMandatorySuccess();
            });

            log.AssertWasCalled(x => x.Trace(procReturn.OutputText));
        }


        [Test]
        public void assert_mandatory_success_with_failures()
        {
            var procReturn = new ProcessReturn()
            {
                ExitCode = 11,
                OutputText = "something"
            };

            var log = MockRepository.GenerateMock<IPackageLog>();
            LogWriter.WithLog(log, procReturn.AssertMandatorySuccess);

            log.AssertWasCalled(x => x.MarkFailure(procReturn.OutputText));
        }

        [Test]
        public void assert_mandatory_success_with_failures_of_negative_number_error_code()
        {
            var procReturn = new ProcessReturn()
            {
                ExitCode = -532462766,
                OutputText = "something"
            };

            var log = MockRepository.GenerateMock<IPackageLog>();
            LogWriter.WithLog(log, procReturn.AssertMandatorySuccess);

            log.AssertWasCalled(x => x.MarkFailure(procReturn.OutputText));
        }

    }
}