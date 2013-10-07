using System.IO;
using System.Threading;
using Bottles.Services.Messaging;
using Bottles.Services.Remote;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;
using SampleService;
using FubuCore;
using Bottles.Services.Messaging.Tracking;

namespace Bottles.Services.Tests.Remote
{
    [TestFixture, Explicit("These tests do not play nice on the build service")]
    public class BigRemoteServicesIntegrationTester
    {
        private RemoteServiceRunner start()
        {
            return new RemoteServiceRunner(x => {
                x.LoadAssemblyContainingType<SampleService.SampleService>();
                x.RequireAssemblyContainingType<BigRemoteServicesIntegrationTester>(); // This is mostly a smoke test
            });
        }

        [Test]
        public void start_with_only_the_folder_name_with_an_IBottleService()
        {
            var servicePath = ".".ToFullPath().ParentDirectory().ParentDirectory().ParentDirectory().AppendPath("SampleService");
            using (var runner = new RemoteServiceRunner(servicePath))
            {
                runner.WaitForServiceToStart<SampleService.SampleService>();
                runner.Started.Any().ShouldBeTrue(); 
            }
        }

        [Test]
        public void start_with_only_the_folder_name_with_an_IApplicationLoader()
        {
            var servicePath = ".".ToFullPath().ParentDirectory().ParentDirectory().ParentDirectory().AppendPath("ApplicationLoaderService");
            using (var runner = new RemoteServiceRunner(servicePath))
            {
                runner.WaitForMessage<LoaderStarted>().LoaderTypeName.ShouldContain("MyApplicationLoader");
            }
        }

        [Test]
        public void start_with_a_parallel_folder()
        {
            using (var runner = new RemoteServiceRunner(x => {
                x.UseParallelServiceDirectory("ApplicationLoaderService");
            }))
            {
                runner.WaitForMessage<LoaderStarted>().LoaderTypeName.ShouldContain("MyApplicationLoader");
            }
        }

        [Test]
        public void run_a_specific_bootstrapper()
        {
            using (var runner = RemoteServiceRunner.For<SampleBootstrapper>())
            {
                runner.WaitForServiceToStart<SampleService.SampleService>();
                runner.WaitForServiceToStart<SampleService.RemoteService>();

                runner.Started.Any().ShouldBeTrue(); 
            }
        }

        [Test]
        public void coordinate_message_history_via_remote_service()
        {
            

            using (var runner = RemoteServiceRunner.For<SampleBootstrapper>())
            {
                runner.WaitForServiceToStart<SampleService.SampleService>();
                runner.WaitForServiceToStart<SampleService.RemoteService>();

                MessageHistory.StartListening(runner);

                var foo = new Foo();

                EventAggregator.SentMessage(foo);


                EventAggregator.Messaging.WaitForMessage<AllMessagesComplete>(() => runner.SendRemotely(foo), 60000)
                                   .ShouldNotBeNull();

            }
        }

        [Test]
        public void spin_up_the_remote_service_for_the_sample_and_send_messages_back_and_forth()
        {
            using (var runner = start())
            {
                runner.WaitForServiceToStart<SampleService.SampleService>();
                runner.WaitForServiceToStart<SampleService.RemoteService>();

                runner.Started.Any().ShouldBeTrue();
            }
        }

        [Test]
        public void spin_up_and_send_and_receive_messages()
        {
            using (var runner = start())
            {
                runner.WaitForServiceToStart<SampleService.RemoteService>();

                runner.WaitForMessage<TestResponse>(() => {
                    runner.SendRemotely(new TestSignal { Number = 1 });
                }).Number.ShouldEqual(1);


                runner.WaitForMessage<TestResponse>(() =>
                {
                    runner.SendRemotely(new TestSignal { Number = 3 });
                }).Number.ShouldEqual(3);

                runner.WaitForMessage<TestResponse>(() =>
                {
                    runner.SendRemotely(new TestSignal { Number = 5 });
                }).Number.ShouldEqual(5);
                
            }
        }
    }
}