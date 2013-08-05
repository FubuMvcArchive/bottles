using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.Services.Messaging;
using FubuCore;

namespace Bottles.Services.Remote
{
    public class RemoteServiceRunner : IDisposable, IListener<ServiceStarted>
    {
        private readonly AppDomain _domain;
        private readonly IMessagingHub _messagingHub = new MessagingHub();
        private readonly RemoteServicesProxy _proxy;
        private readonly RemoteListener _remoteListener;
        private readonly IList<ServiceStarted> _started = new List<ServiceStarted>();

        public RemoteServiceRunner(Action<RemoteDomainExpression> configure)
        {
            

            var expression = new RemoteDomainExpression();
            configure(expression);

            _messagingHub.AddListener(this);
            AppDomainSetup setup = expression.Setup;

            _domain = AppDomain.CreateDomain(expression.Setup.ApplicationName, null, setup);



            expression.As<IAssemblyMover>().MoveAssemblies(setup);


            Type proxyType = typeof (RemoteServicesProxy);
            _proxy = (RemoteServicesProxy)
                     _domain.CreateInstanceAndUnwrap(proxyType.Assembly.FullName, proxyType.FullName);

            _remoteListener = new RemoteListener(_messagingHub);


            _proxy.Start(expression.BootstrapperName, _remoteListener);
        }

        public IEnumerable<ServiceStarted> Started
        {
            get { return _started; }
        }

        public IMessagingHub Messaging
        {
            get { return _messagingHub; }
        }

        public void Dispose()
        {
            _proxy.Shutdown();
            AppDomain.Unload(_domain);
        }

        void IListener<ServiceStarted>.Receive(ServiceStarted message)
        {
            _started.Add(message);
        }

        public void WaitForServiceToStart<T>() where T : IActivator
        {
            Wait.Until(() => { return _started.Any(x => x.ActivatorTypeName == typeof (T).AssemblyQualifiedName); });
        }

        public void SendRemotely<T>(T message)
        {
            string json = MessagingHub.ToJson(message);
            _proxy.SendJson(json);
        }

        public T WaitForMessage<T>(Action action, int wait = 5000)
        {
            return WaitForMessage<T>(t => true, action, wait);
        }

        public T WaitForMessage<T>(Func<T, bool> filter, Action action, int wait = 5000)
        {
            return _remoteListener.WaitForMessage(filter, action, wait);
        }

        public static RemoteServiceRunner For<T>(Action<RemoteDomainExpression> configure = null) where T : IBootstrapper
        {
            if (configure == null)
            {
                configure = x => { };
            }

            return new RemoteServiceRunner(x => {
                configure(x);
                x.BootstrapperName = typeof (T).AssemblyQualifiedName;
            });
        }
    }
}