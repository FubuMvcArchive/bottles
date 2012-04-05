using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bottles.Diagnostics;

namespace Bottles.PackageLoaders.Assemblies
{
    public class AssemblyRegistration : IAssemblyRegistration
    {

        public static readonly string DIRECTLY_REGISTERED_MESSAGE = "Directly loaded by the Package";

        private readonly IPackagingDiagnostics _diagnostics;
        private readonly IList<Assembly> _assemblies = new List<Assembly>();
        private IPackageInfo _currentPackage;

        public AssemblyRegistration(Func<string, Assembly> loader, IPackagingDiagnostics diagnostics)
        {
            AssemblyFileLoader = loader;
            _diagnostics = diagnostics;
        }

        //why is this a public function?
        //so it can be overridden in tests is one reason
        // ? is it used in fubumvc?
        // can't I just pass it in the ctor?
        public Func<string, Assembly> AssemblyFileLoader { get; set; }

        // need to try to load the assembly by name first!!!
        public void LoadFromFile(string fileName, string assemblyName)
        {
            if (hasAssemblyByName(assemblyName))
            {
                _diagnostics.LogDuplicateAssembly(_currentPackage, assemblyName);
            }
            else
            {
                try
                {
                    var assembly = AssemblyFileLoader(fileName);
                    _diagnostics.LogAssembly(_currentPackage, assembly, "Loaded from " + fileName);

                    _assemblies.Add(assembly);
                }
                catch (Exception e)
                {
                    _diagnostics.LogAssemblyFailure(_currentPackage, fileName, e);
                }
            }


        }

        public void SetCurrentPackage(IPackageInfo packageInfo)
        {
            _currentPackage = packageInfo;
        }

        public void Use(Assembly assembly)
        {
            if (hasAssemblyByName(assembly.GetName().Name))
            {
                _diagnostics.LogDuplicateAssembly(_currentPackage, assembly.GetName().FullName);
                return;
            }

            _diagnostics.LogAssembly(_currentPackage, assembly, DIRECTLY_REGISTERED_MESSAGE);
            _assemblies.Add(assembly);
        }

        public IEnumerable<Assembly> Assemblies
        {
            get { return _assemblies; }
        }

        bool hasAssemblyByName(string assemblyName)
        {
            // I know, packaging *ONLY* supporting one version of a dll.  Use older stuff to 
            // make redirects go
            return (_assemblies.Any(x => x.GetName().Name == assemblyName));
        }
    }
}