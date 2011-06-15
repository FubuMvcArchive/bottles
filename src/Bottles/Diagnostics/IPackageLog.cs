using System;
using System.Collections.Generic;
using FubuCore.CommandLine;
using FubuCore;

namespace Bottles.Diagnostics
{
    public interface IPackageLog
    {
        void Trace(ConsoleColor color, string text, params object[] parameters);
        void Trace(string text, params object[] parameters);

        void MarkFailure(Exception exception);
        void MarkFailure(string text);
        string FullTraceText();
        string Description { get; }
        bool Success { get; }
        long TimeInMilliseconds { get; }
    }

    public static class IPackageLogExtensions
    {
        public static void TrapErrors(this IPackageLog log, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                log.MarkFailure(e);
            }
        }
    }

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

    public static class LogWriter
    {
        private static readonly LogWriterStatus _status = new LogWriterStatus();
        private static int _totalSteps = 0;
        private static int _currentStep = 0;

        public static void StartSteps(int totalCount, string header)
        {
            _totalSteps = totalCount;
            _currentStep = 1;
            
            
            ConsoleWriter.Write(ConsoleColor.White, header);
        }

        public static void RunningStep(string format, params object[] parameters)
        {
            var length = _totalSteps.ToString().Length;
            var count = "  - {0} / {1}:  ".ToFormat(_currentStep.ToString().PadLeft(length, ' '), _totalSteps);
            ConsoleWriter.Write(ConsoleColor.Gray, count + format.ToFormat(parameters));

            _currentStep++;
        }


        public static void WithLog(IPackageLog log, Action action)
        {
            _status.PushLog(log);
            try
            {
                action();
            }
            finally
            {
                _status.PopLog();
            }
        }

        private static int _indent = 0;

        public static void Indent()
        {
            _indent++;
        }

        public static void Unindent()
        {
            _indent--;
        }

        public static void Indent(Action action)
        {
            Indent();
            try
            {
                action();
            }
            finally
            {
                Unindent();
            }
        }

        public static T Indent<T>(Func<T> action)
        {
            Indent();
            try
            {
                return action();
            }
            finally
            {
                Unindent();
            }
        }

        public static void Write(ConsoleColor color, string format, params object[] parameters)
        {
            format = indentFormat(format);

            _status.Current.Trace(color, format, parameters);
        }

        private static string indentFormat(string format)
        {
            var spaces = _indent > 0 ? string.Empty.PadRight(_indent*2, ' ') : string.Empty;
            format = spaces + format;
            return format;
        }

        public static void Header1(string format, params object[] parameters)
        {
            var text = format.ToFormat(parameters);

            var line = "".PadRight(text.Length, '-');
            Write(ConsoleColor.White, line);
            Header2(format, parameters);
            Write(ConsoleColor.White, line);
        }

        public static void Header2(string format, params object[] parameters)
        {
            Write(ConsoleColor.White, format, parameters);
        }

        public static void Highlight(string format, params object[] parameters)
        {
            Write(ConsoleColor.Cyan, format, parameters);
        }

        public static void Success(string format, params object[] parameters)
        {
            Write(ConsoleColor.Green, format, parameters);
        }

        public static void Fail(string format, params object[] parameters)
        {
            format = indentFormat(format);
            _status.Current.MarkFailure(format.ToFormat(parameters));
        }

        public static void Trace(string format, params object[] parameters)
        {
            Write(ConsoleColor.Gray, format, parameters);
        }

        public static void PrintHorizontalLine()
        {
            Write(ConsoleColor.White, "".PadRight(80, '-'));
        }
    }
}