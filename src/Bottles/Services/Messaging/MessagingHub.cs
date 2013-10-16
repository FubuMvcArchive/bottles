using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;
using Newtonsoft.Json;

namespace Bottles.Services.Messaging
{
    public class MessagingHub : IMessagingHub
    {
        public static string ToJson(object o)
        {
            var serializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.All };

            var writer = new StringWriter();
            serializer.Serialize(writer, o);

            return writer.ToString();
        }

        // TODO -- need to do some locking on this bad boy
        private readonly IList<object> _listeners = new List<object>();
        private readonly JsonSerializer _jsonSerializer = new JsonSerializer()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public IEnumerable<object> Listeners
        {
            get { return _listeners; }
        }

        public void AddListener(object listener)
        {
            _listeners.Fill(listener);
        }

        public void RemoveListener(object listener)
        {
            _listeners.Remove(listener);
        }

        public void Send<T>(T message)
        {
            _listeners.OfType<IListener<T>>().Each(x => x.Receive(message));
            _listeners.OfType<IListener>().Each(x => x.Receive(message));
        }

        public void SendJson(string json)
        {
            var o = _jsonSerializer.Deserialize(new JsonTextReader(new StringReader(json)));
            typeof(Sender<>).CloseAndBuildAs<ISender>(o.GetType())
                            .Send(o, this);
        }

        public interface ISender
        {
            void Send(object o, MessagingHub listeners);
        }

        public class Sender<T> : ISender
        {
            public void Send(object o, MessagingHub listeners)
            {
                listeners.Send(o.As<T>());
            }
        }
    }
}