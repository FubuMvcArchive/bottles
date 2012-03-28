using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Bottles.Diagnostics;
using Bottles.PackageLoaders.Assemblies;
using FubuCore;
using FubuCore.Reflection;

namespace Bottles
{
    //rename to bottles registry
    public static class PackageRegistry
    {
        private static readonly IList<Assembly> _assemblies = new List<Assembly>();
        private static readonly IList<IPackageInfo> _packages = new List<IPackageInfo>();

        static PackageRegistry()
        {
            /* 
             * This is a critical - KEY - concept
             * read up on it before making changes
             * http://msdn.microsoft.com/en-us/library/system.appdomain.assemblyresolve.aspx
             */
            AppDomain.CurrentDomain.AssemblyResolve += findAssemblyInLoadedPackages;
                

            GetApplicationDirectory = () => AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// Critical method in the bottles eco system
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly findAssemblyInLoadedPackages(object sender, ResolveEventArgs args)
        {
            var theMissingAssemblyName = args.Name;
            return _assemblies.FirstOrDefault(assembly => theMissingAssemblyName == assembly.GetName().Name ||
                                                          theMissingAssemblyName == assembly.GetName().FullName);
        }

        //REVIEW: This really feels wrong - if its required can we make it an argument of LoadPackages("app dir", cfg=>{});
        public static Func<string> GetApplicationDirectory { get; set; }

        /// <summary>
        /// All the assemblies found in all packages
        /// </summary>
        public static IEnumerable<Assembly> PackageAssemblies
        {
            get { return _assemblies; }
        }

        /// <summary>
        /// Packages that have been loaded
        /// </summary>
        public static IEnumerable<IPackageInfo> Packages
        {
            get { return _packages; }
        }

        /// <summary>
        /// The Diagnostics of the package environment
        /// </summary>
        public static PackagingDiagnostics Diagnostics { get; private set; }

        /// <summary>
        /// The entry method into the bottles environment
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="runActivators"></param>
        public static void LoadPackages(Action<IPackageFacility> configuration, bool runActivators = true)
            //consider renaming to InitializeEnvironment
            //have it return an environment object.
        {
            _packages.Clear();

            Diagnostics = new PackagingDiagnostics();
            var record = new PackageLoadingRecord();

            Diagnostics.LogExecution(record, () =>
            {
                var facility = new PackageFacility();
                var assemblyLoader = new AssemblyLoader(Diagnostics);
                var graph = new PackagingRuntimeGraph(Diagnostics, assemblyLoader, _packages);

                var codeLocation = findCallToLoadPackages();
                graph.InProvenance(codeLocation, g =>
                {
                    //collect user configuration
                    configuration(facility);

                    //applies collected configurations
                    facility.Configure(g);
                });


                graph.DiscoverAndLoadPackages(() =>
                {
                    //clearing assemblies why? - my guess is testing.
                    // this should only really be called once.
                    _assemblies.Clear();

                    _assemblies.AddRange(assemblyLoader.Assemblies);
                    //the above assemblies are used when we need to resolve bottle assemblies
                }, runActivators);
            });

            record.Finished = DateTime.Now;
        }

        private static string findCallToLoadPackages()
        {
            var theBottleAssembly = typeof (IPackageInfo).Assembly; //bottle assembly
            var trace = new StackTrace(Thread.CurrentThread, false);

            //walk the stack looking for the first 'valid' frame to use
            for (var i = 0; i < trace.FrameCount; i++)
            {
                var frame = trace.GetFrame(i);
                var assembly = frame.GetMethod().DeclaringType.Assembly;

                if (assembly == theBottleAssembly) continue;
                if (!frame.GetMethod().HasAttribute<SkipOverForProvenanceAttribute>()) continue;


                return frame.ToDescription();
            }

            return "Unknown";
        }

        /// <summary>
        /// A static method that should be exposed, to allow you to 
        /// take an action when there has been a failure in the system.
        /// </summary>
        /// <param name="failure">The action to perform</param>
        public static void AssertNoFailures(Action failure)
        {
            if (Diagnostics.HasErrors())
            {
                failure();
            }
        }

        /// <summary>
        /// Default AssertNoFailures
        /// </summary>
        public static void AssertNoFailures()
        {
            AssertNoFailures(() =>
            {
                var writer = new StringWriter();
                writer.WriteLine("Package loading and application bootstrapping failed");
                writer.WriteLine();
                Diagnostics.EachLog((o, log) =>
                {
                    if (!log.Success)
                    {
                        writer.WriteLine(o.ToString());
                        writer.WriteLine(log.FullTraceText());
                        writer.WriteLine("------------------------------------------------------------------------------------------------");
                    }
                });

                throw new ApplicationException(writer.GetStringBuilder().ToString());
            });
        }
    }
}