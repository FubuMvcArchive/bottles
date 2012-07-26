using System;
using System.Collections.Generic;
using System.Linq;
using Bottles.Diagnostics;
using Bottles.PackageLoaders.Assemblies;
using FubuCore;

namespace Bottles
{
    /// <summary>
    /// A collection of data about a given runtime
    /// </summary>
    public class BottlingRuntimeGraph : IDisposable
    {
        private readonly IList<IActivator> _activators = new List<IActivator>();
        private readonly IAssemblyLoader _assemblies;
        private readonly IList<IBootstrapper> _bootstrappers = new List<IBootstrapper>();
        private readonly IBottlingDiagnostics _diagnostics;
        private readonly IList<IBottleLoader> _packageLoaders = new List<IBottleLoader>();
        private readonly Stack<string> _provenanceStack = new Stack<string>();
        private readonly IList<IBottleInfo> _packages;

        public BottlingRuntimeGraph(IBottlingDiagnostics diagnostics, IAssemblyLoader assemblies, IList<IBottleInfo> packages)
        {
            _diagnostics = diagnostics;
            _assemblies = assemblies;
        	_packages = packages;
        }


        public IDisposable InProvenance(string provenance, Action<BottlingRuntimeGraph> action)
        {
            PushProvenance(provenance);
            action(this);
            return this;
        }

        public void Dispose()
        {
            PopProvenance();
        }

        public void PushProvenance(string provenance)
        {
            _provenanceStack.Push(provenance);
        }


        public void PopProvenance()
        {
            _provenanceStack.Pop();
        }

        //I kinda want this method elsewhere
        public void DiscoverAndLoadPackages(Action onAssembliesScanned, bool runActivators = true)
        {
            var allPackages = findAllPackages();

            //orders _packages
            analyzePackageDependenciesAndOrder(allPackages);

            loadAssemblies(_packages, onAssembliesScanned);
            var discoveredActivators = collectAllActivatorsFromBootstrappers();

            if(runActivators)
            {
                activatePackages(_packages, discoveredActivators);    
            }
        }

        private void analyzePackageDependenciesAndOrder(IEnumerable<IBottleInfo> packages)
        {
            var dependencyProcessor = new BottleDependencyProcessor(packages);
            dependencyProcessor.LogMissingPackageDependencies(_diagnostics);
            _packages.AddRange(dependencyProcessor.OrderedPackages());
        }

        private void activatePackages(IList<IBottleInfo> packages, IList<IActivator> discoveredActivators)
        {
            var discoveredPlusRegisteredActivators = discoveredActivators.Union(_activators);
            _diagnostics.LogExecutionOnEach(discoveredPlusRegisteredActivators, (activator, log) => activator.Activate(packages, log));
        }

        public void AddBootstrapper(IBootstrapper bootstrapper)
        {
            _bootstrappers.Add(bootstrapper);
            _diagnostics.LogObject(bootstrapper, currentProvenance);
        }

        public void AddLoader(IBottleLoader loader)
        {
            _packageLoaders.Add(loader);
            _diagnostics.LogObject(loader, currentProvenance);
        }

        public void AddActivator(IActivator activator)
        {
            _activators.Add(activator);
            _diagnostics.LogObject(activator, currentProvenance);
        }

        public void AddFacility(IBottleFacility facility)
        {
            _diagnostics.LogObject(facility, currentProvenance);

            PushProvenance(facility.ToString());
            
            facility.As<IPackagingRuntimeGraphConfigurer>().Configure(this);
            PopProvenance();
        }

        private string currentProvenance
        {
            get { return _provenanceStack.Peek(); }
        }

        private List<IActivator> collectAllActivatorsFromBootstrappers()
        {
            var result = new List<IActivator>();
            
            _diagnostics.LogExecutionOnEach(_bootstrappers, (currentBootstrapper, log) =>
            {
                var bootstrapperActivators = currentBootstrapper.Bootstrap(log);
                result.AddRange(bootstrapperActivators);
                _diagnostics.LogBootstrapperRun(currentBootstrapper, bootstrapperActivators);
            });

            return result;
        }

        private void loadAssemblies(IEnumerable<IBottleInfo> packages, Action onAssembliesScanned)
        {
            _diagnostics.LogExecutionOnEach(packages, _assemblies.ReadPackage);

            onAssembliesScanned();
        }

        private IEnumerable<IBottleInfo> findAllPackages()
        {
            var result = new List<IBottleInfo>();

            _diagnostics.LogExecutionOnEach(_packageLoaders, (currentLoader, log) =>
            {
                var packageInfos = currentLoader.Load(log).ToArray();
                _diagnostics.LogPackages(currentLoader, packageInfos);

                result.AddRange(packageInfos);
            });

            return result;
        }
    }
}