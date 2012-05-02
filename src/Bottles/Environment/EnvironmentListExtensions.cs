using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles.Environment
{
    public static class EnvironmentListExtensions
    {
        public static void Add(this IList<EnvironmentLogEntry> list, object target, PackageLog log)
        {
            list.Add(EnvironmentLogEntry.FromPackageLog(target, log));
        }
    }
}