using System;
using System.Collections.Generic;
using System.Text;
using Bottles.Diagnostics;
using FubuCore.DependencyAnalysis;
using System.Linq;
using FubuCore;

namespace Bottles
{
    public class BottleDependencyProcessor
    {
        private readonly IEnumerable<IBottleInfo> _packages;
        private readonly DependencyGraph<IBottleInfo> _graph;

        public BottleDependencyProcessor(IEnumerable<IBottleInfo> packages)
        {
            _packages = packages;

            guardAgainstMalformedPackages();

            _graph = new DependencyGraph<IBottleInfo>(pak => pak.Name, pak => pak.Dependencies.Select(x => x.Name));

            _packages.OrderBy(p => p.Name).Each(p => _graph.RegisterItem(p));
        }

        public void LogMissingPackageDependencies(IBottlingDiagnostics diagnostics)
        {
            var missingDependencies = _graph.MissingDependencies();
            missingDependencies.Each(name =>
            {
                var dependentPackages = _packages.Where(pak => pak.Dependencies.Any(dep => dep.IsMandatory && dep.Name == name));
                dependentPackages.Each(pak => diagnostics.LogFor(pak).LogMissingDependency(name));
            });
        }

        public IEnumerable<IBottleInfo> OrderedPackages()
        {
            return _graph.Ordered();
        }

        private void guardAgainstMalformedPackages()
        {
            var missing = _packages.Where(p => p.Name.IsEmpty());
            if (missing.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine("{0} bottles are missing a name".ToFormat(missing.Count()));
                missing.Each(pi =>
                                 {
                                     sb.AppendLine(pi.Description);
                                 });
                
                throw new ArgumentException(sb.ToString());
            }
        }
    }

    public static class BottleLogDependencyExtensions
    {
        public static void LogMissingDependency(this IBottleLog log, string dependencyName)
        {
            log.MarkFailure("Missing required Bottle/Package dependency named '{0}'".ToFormat(dependencyName));
        }
    }
}