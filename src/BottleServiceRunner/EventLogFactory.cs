using System;
using System.Diagnostics;
using Bottles.Services;
using Topshelf.HostConfigurators;
using Topshelf.Logging;

namespace BottleServiceRunner
{
    public static class HostConfiguratorLogExtensions
    {
        public static void UseEventLog(this HostConfigurator configurator, BottleServiceConfiguration settings)
        {
            HostLogger.UseLogger(new EventLogFactoryConfigurator(settings));
        }

        public class EventLogFactoryConfigurator : HostLoggerConfigurator
        {
            private readonly BottleServiceConfiguration _settings;

            public EventLogFactoryConfigurator(BottleServiceConfiguration settings)
            {
                _settings = settings;
            }

            public LogWriterFactory CreateLogWriterFactory()
            {
                return new EventLogFactory(_settings);
            }
        }
    }

    public class EventLogFactory : LogWriterFactory
    {
        private readonly BottleServiceConfiguration _settings;

        public EventLogFactory(BottleServiceConfiguration settings)
        {
            _settings = settings;
        }

        public LogWriter Get(string name)
        {
            return new EventLogWriter(_settings, name);
        }

        public void Shutdown()
        {
            //no-op
        }
    }

    public class EventLogWriter : LogWriter
    {
        private readonly BottleServiceConfiguration _settings;
        private readonly string _name;

        public EventLogWriter(BottleServiceConfiguration settings, string name)
        {
            _settings = settings;
            _name = name;
        }

        public void Log(LoggingLevel level, object obj)
        {
            // no-op
        }

        public void Log(LoggingLevel level, object obj, Exception exception)
        {
            // no-op
        }

        public void Log(LoggingLevel level, LogWriterOutputProvider messageProvider)
        {
            // no-op
        }

        public void LogFormat(LoggingLevel level, IFormatProvider formatProvider, string format, params object[] args)
        {
            // no-op
        }

        public void LogFormat(LoggingLevel level, string format, params object[] args)
        {
            // no-op
        }

        public void Debug(object obj)
        {
            // no-op
        }

        public void Debug(object obj, Exception exception)
        {
            // no-op
        }

        public void Debug(LogWriterOutputProvider messageProvider)
        {
            // no-op
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            // no-op
        }

        public void DebugFormat(string format, params object[] args)
        {
            // no-op
        }

        public void Info(object obj)
        {
            // no-op
        }

        public void Info(object obj, Exception exception)
        {
            // no-op
        }

        public void Info(LogWriterOutputProvider messageProvider)
        {
            // no-op
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            // no-op
        }

        public void InfoFormat(string format, params object[] args)
        {
            // no-op
        }

        public void Warn(object obj)
        {
            // no-op
        }

        public void Warn(object obj, Exception exception)
        {
            // no-op
        }

        public void Warn(LogWriterOutputProvider messageProvider)
        {
            // no-op
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            // no-op
        }

        public void WarnFormat(string format, params object[] args)
        {
            // no-op
        }

        public void Error(object obj)
        {
            // no-op
        }

        public void Error(object obj, Exception exception)
        {
            EventLog.WriteEntry(_settings.Name, string.Format("[{0}]: {1}", _name, obj));
            EventLog.WriteEntry(_settings.Name, string.Format("[{0}]: {1}", _name, exception.ToString()));
        }

        public void Error(LogWriterOutputProvider messageProvider)
        {
            // no-op
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            // no-op
        }

        public void ErrorFormat(string format, params object[] args)
        {
            // no-op
        }

        public void Fatal(object obj)
        {
            // no-op
        }

        public void Fatal(object obj, Exception exception)
        {
            // no-op
        }

        public void Fatal(LogWriterOutputProvider messageProvider)
        {
            // no-op
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            // no-op
        }

        public void FatalFormat(string format, params object[] args)
        {
            // no-op
        }

        public bool IsDebugEnabled { get { return false; } }
        public bool IsInfoEnabled { get { return false; } }
        public bool IsWarnEnabled { get { return false; } }
        public bool IsErrorEnabled { get { return false; } }
        public bool IsFatalEnabled { get { return true; } }
    }
}