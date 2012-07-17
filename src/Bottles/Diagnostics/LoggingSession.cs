using System;
using System.Linq;
using FubuCore.Util;

namespace Bottles.Diagnostics
{
    public class LoggingSession
    {
        private readonly Cache<object, BottleLog> _logs = new Cache<object, BottleLog>(o => new BottleLog{
            Description = o.ToString()
        });

        public void LogObject(object target, string provenance)
        {
            _logs[target].Provenance = provenance;
        }

        public IBottleLog LogFor(object target)
        {
            return _logs[target];
        }

        public void LogExecution(object target, Action continuation)
        {
            _logs[target].Execute(continuation);
        }

        public void EachLog(Action<object, BottleLog> action)
        {
            _logs.Each(action);
        }

        public bool HasErrors()
        {
            return _logs.GetAll().Any(x => !x.Success);
        }
    }
}