using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bottles;
using Bottles.Diagnostics;
using Bottles.Services.Messaging;
using StructureMap;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using IBootstrapper = Bottles.IBootstrapper;

namespace SampleService
{
    public class Foo
    {
        public Foo()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
    }

    public class SampleBootstrapper : IBootstrapper
    {
        public IEnumerable<IActivator> Bootstrap(IPackageLog log)
        {
            ObjectFactory.Initialize(x => x.AddRegistry<SampleRegistry>());
            yield return ObjectFactory.GetInstance<SampleService>();
            yield return ObjectFactory.GetInstance<RemoteService>();
        }
    }

    public class SampleRegistry : Registry
    {
        public SampleRegistry()
        {
            Scan(x => {
                x.TheCallingAssembly();
                x.WithDefaultConventions();
            });
        }
    }

    public interface IDependency
    {
    }

    public class Dependency : IDependency
    {
    }

    public class SampleService : IActivator, IDeactivator, IListener<Foo>
    {
        private readonly IDependency _dependency;

        public SampleService(IDependency dependency)
        {
            _dependency = dependency;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            Write("Starting SampleService...");

            EventAggregator.Messaging.AddListener(this);
        }

        public void Deactivate(IPackageLog log)
        {
            Write("Stopping SampleService...");
        }

        public void Write(string message)
        {
            Console.WriteLine("From {0}: {1}", GetType().Name, message);
        }

        public void Receive(Foo message)
        {
            Task.Factory.StartNew(() => {
                Thread.Sleep(500);
                EventAggregator.ReceivedMessage(message);
            });
        }
    }

    public class RemoteService : IActivator, IDeactivator, IListener<TestSignal>
    {
        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            EventAggregator.Messaging.AddListener(this);
        }


        public void Deactivate(IPackageLog log)
        {
        }

        public void Receive(TestSignal message)
        {
            EventAggregator.SendMessage(new TestResponse {Number = message.Number});
        }
    }

    public class TestSignal
    {
        public int Number { get; set; }

        protected bool Equals(TestSignal other)
        {
            return Number == other.Number;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TestSignal) obj);
        }

        public override int GetHashCode()
        {
            return Number;
        }

        public override string ToString()
        {
            return string.Format("Signal: {0}", Number);
        }
    }

    public class TestResponse
    {
        public int Number { get; set; }

        protected bool Equals(TestResponse other)
        {
            return Number == other.Number;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TestResponse) obj);
        }

        public override int GetHashCode()
        {
            return Number;
        }

        public override string ToString()
        {
            return string.Format("Response: {0}", Number);
        }
    }
}