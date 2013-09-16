using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Bottles.Diagnostics;
using Bottles.PackageLoaders.Assemblies;
using FubuCore;

namespace Bottles.Services
{

    /// <summary>
    /// Bottles IPackageLoader that finds and loads assemblies in the application binary directory marked
    /// with the [BottleService] attribute
    /// </summary>
    [Description("Loads assemblies marked with the [BottleService] attribute")]
    public class BottleServicePackageLoader : IPackageLoader
    {
        public IEnumerable<IPackageInfo> Load(IPackageLog log)
        {
            Func<Assembly, bool> filter = assem => assem.GetCustomAttributes(typeof(BottleServiceAttribute), false).Any();
            Action<string> onDirectoryFound = dir => log.Trace("Looking for assemblies marked with the [BottleService] attribute in " + dir);
            var assemblies = AssemblyFinder.FindAssemblies(filter, onDirectoryFound);

            return assemblies.Select(x => new AssemblyPackageInfo(x));
        }
    }
}