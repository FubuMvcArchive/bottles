using System.Collections.Generic;

namespace Bottles.Diagnostics
{
    /// <summary>
    /// Manages the log stack
    /// </summary>
    public class LogWriterStatus
    {
        private readonly Stack<IBottleLog> _logs = new Stack<IBottleLog>();

        public LogWriterStatus()
        {
            _logs.Push(new BottleLog());
        }

        public IBottleLog Current
        {
            get
            {
                return _logs.Peek();
            }
        }

        public void PushLog(IBottleLog log)
        {
            _logs.Push(log);
        }

        public void PopLog()
        {
            _logs.Pop();
        }
    }
}