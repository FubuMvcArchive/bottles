using System;
using System.Collections.Generic;

namespace Bottles.Diagnostics
{
    public static class PackagingDiagnosticsExtensions
    {
        public static void LogExecutionOnEach<TItem>(this IPackagingDiagnostics diagnostics, IEnumerable<TItem> targets, Action<TItem, IPackageLog> continuation)
        {
            targets.Each(currentTarget =>
            {
                var log = diagnostics.LogFor(currentTarget);
                diagnostics.LogExecution(currentTarget, () => continuation(currentTarget, log));
            });
        }

        public static void LogPackages(this IPackagingDiagnostics diagnostics, IPackageLoader loader, IEnumerable<IPackageInfo> packages)
        {
            packages.Each(p =>
            {
                diagnostics.LogPackage(p, loader);
            });
        }
    }
}