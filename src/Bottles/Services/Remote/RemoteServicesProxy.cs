using System;
using Bottles.Services.Messaging;

namespace Bottles.Services.Remote
{
    public class RemoteServicesProxy : MarshalByRefObject
    {
        private IDisposable _shutdown;

        public void Start(string bootstrapperName, MarshalByRefObject remoteListener)
        {
            var domainSetup = AppDomain.CurrentDomain.SetupInformation;
            System.Environment.CurrentDirectory = domainSetup.ApplicationBase;
             
            // TODO -- need to handle exceptions gracefully here
            EventAggregator.Start((IRemoteListener) remoteListener);

            var loader = BottleServiceApplication.FindLoader(bootstrapperName);
            _shutdown = loader.Load();
        }

        public void Shutdown()
        {
            EventAggregator.Stop();
            _shutdown.Dispose();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void SendJson(string json)
        {
            EventAggregator.Messaging.SendJson(json);
        }
    }
}