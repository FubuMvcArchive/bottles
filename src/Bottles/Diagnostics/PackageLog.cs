using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Diagnostics
{
    [Serializable]
    public class PackageLog : IPackageLog
    {
        private readonly StringWriter _text = new StringWriter();
        private readonly IList<object> _children = new List<object>();

        public PackageLog()
        {
            Success = true;
            Id = Guid.NewGuid();
        }

        public long TimeInMilliseconds { get; set; }
        public string Provenance { get; set; }
        public string Description { get; set; }

        public void Execute(Action continuation)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                continuation();
            }
            catch (Exception e)
            {
                MarkFailure(e);
            }
            finally
            {
                stopwatch.Stop();
                TimeInMilliseconds = stopwatch.ElapsedMilliseconds;
            }
        }

        public void Trace(string text)
        {
            ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, 4, text);
            _text.WriteLine(text);
        }

        public void Trace(string format, params object[] parameters)
        {
            Trace(format.ToFormat(parameters));
        }

        public bool Success { get; private set; }

        public void MarkFailure(Exception exception)
        {
            MarkFailure(exception.ToString());
        }

        public void MarkFailure(string text)
        {
            ConsoleWriter.Write(ConsoleColor.Red, text);

            _text.WriteLine(text);
            
            Success = false;
        }

        public string FullTraceText()
        {
            return _text.ToString();
        }

        public void AddChild(params object[] child)
        {
            _children.AddRange(child);
        }

        public IEnumerable<T> FindChildren<T>()
        {
            return _children.Where(x => x is T).Cast<T>();
        }

        public Guid Id
        {
            get; private set;
        }


    }
}