using System;
using Bottles.Diagnostics;

namespace Bottles.Environment
{
    [Serializable]
    public class EnvironmentLogEntry
    {
        public bool Success { get; set; }
        public string TraceText { get; set; }
        public string Description { get; set; }
        public long TimeInMilliseconds { get; set; }

        public static EnvironmentLogEntry FromPackageLog(object target, IPackageLog log)
        {
            return new EnvironmentLogEntry(){
                Description = target.ToString(),
                Success = log.Success,
                TraceText = log.FullTraceText().Trim(),
                TimeInMilliseconds = log.TimeInMilliseconds
            };
        }

    }
}