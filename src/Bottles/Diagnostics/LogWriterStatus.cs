using System;
using System.Collections.Generic;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Diagnostics
{
    public class LogWriterStatus : IPackageLog
    {
        private readonly Stack<IPackageLog> _logs = new Stack<IPackageLog>();

        public LogWriterStatus()
        {
            _logs.Push(this);
        }

        public IPackageLog Current
        {
            get
            {
                return _logs.Peek();
            }
        }

        public void PushLog(IPackageLog log)
        {
            _logs.Push(log);
        }

        public void PopLog()
        {
            _logs.Pop();
        }


        #region IPackageLog implementations

        public void Trace(ConsoleColor color, string text, params object[] parameters)
        {
            ConsoleWriter.Write(color, text.ToFormat(parameters));
        }

        public void Trace(string text, params object[] parameters)
        {
            Trace(ConsoleColor.Gray, text, parameters);
        }

        void IPackageLog.MarkFailure(Exception exception)
        {
            
        }

        void IPackageLog.MarkFailure(string text)
        {
            ConsoleWriter.Write(ConsoleColor.Red, text);
        }

        string IPackageLog.FullTraceText()
        {
            throw new NotImplementedException();
        }

        string IPackageLog.Description
        {
            get { throw new NotImplementedException(); }
        }

        bool IPackageLog.Success
        {
            get { throw new NotImplementedException(); }
        }

        long IPackageLog.TimeInMilliseconds
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

    }
}