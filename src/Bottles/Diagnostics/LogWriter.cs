using System;

namespace Bottles.Diagnostics
{
    /// <summary>
    /// Provides static access to the 'current' IBottleLog
    /// </summary>
    public static class LogWriter
    {
        private static readonly LogWriterStatus _status = new LogWriterStatus();

        public static void WithLog(IBottleLog log, Action action)
        {
            _status.PushLog(log);
            try
            {
                log.Execute(action);
            }
            catch (Exception ex)
            {
                log.MarkFailure(ex);
            }
            finally
            {
                _status.PopLog();
            }
        }

        public static IBottleLog Current
        {
            get { return _status.Current; }
        }
    }
}