using Bottles.Diagnostics;
using FubuCore.CommandLine;

namespace Bottles
{
    public interface ISimpleLogger
    {
        void Log(string text, params object[] parameters);
    }

    public class SimpleLogger : ISimpleLogger
    {
        public void Log(string text, params object[] parameters)
        {
            LogWriter.Trace(text, parameters);
        }
    }
}