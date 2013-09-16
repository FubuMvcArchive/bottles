using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Services
{
    public static class BottleServiceFinder
    {
        public static IEnumerable<IBootstrapper> FindBootstrappers(IEnumerable<Assembly> packageAssemblies)
        {
            var bootstrappers = packageAssemblies.FilterTypes(type => type.CanBeCastTo<IBootstrapper>() && type.IsConcreteWithDefaultCtor() && type != typeof(BottleServiceBootstrapper));
            return bootstrappers.Select(x => (IBootstrapper) Activator.CreateInstance(x));
        }

        public static IEnumerable<IBottleService> Find(IEnumerable<Assembly> packageAssemblies, IPackageLog log)
        {
            var bootstrappers = FindBootstrappers(packageAssemblies).ToArray();
            Console.WriteLine("Found {0} bootstrappers".ToFormat(bootstrappers.Count()));
            bootstrappers.Each(x => Console.WriteLine(x));

            return bootstrappers
                .SelectMany(x => x.Bootstrap(log))
                .Where(BottleService.IsBottleService)
                .Select(x => new BottleService(x, log))
                .ToList();
        }

        public static IEnumerable<Type> FindTypes(IEnumerable<Assembly> packageAssemblies)
        {
            return packageAssemblies.FilterTypes(BottleService.IsBottleService);
        }

        public static IEnumerable<Type> FilterTypes(this IEnumerable<Assembly> packageAssemblies, Func<Type, bool> predicate)
        {
            var filteredTypes = new List<Type>();

            packageAssemblies.Each(x =>
            {
                try
                {
                    var types = x.GetExportedTypes();
                    filteredTypes.AddRange(types.Where(predicate));
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Unable to find exported types from assembly " + x.FullName, ex);
                }
            });

            return filteredTypes;
        }
    }
}