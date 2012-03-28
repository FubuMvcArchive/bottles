using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.PackageLoaders.Assemblies
{
    /// <summary>
    /// Default assembly loader
    /// </summary>
    public class AssemblyLoader : 
        IAssemblyLoader, 
        IAssemblyRegistration //why is this playing TWO roles?
    {
        public static readonly string DIRECTLY_REGISTERED_MESSAGE = "Directly loaded by the Package";

        private readonly IPackagingDiagnostics _diagnostics;
        private IPackageInfo _currentPackage;
        private readonly IList<Assembly> _assemblies = new List<Assembly>();

        public AssemblyLoader(IPackagingDiagnostics diagnostics)
        {
            AssemblyFileLoader = loadPackageAssemblyFromAppBinPath;
            _diagnostics = diagnostics;
        }


        //why is this a public function?
        //so it can be overridden in tests is one reason
        // ? is it used in fubumvc?
        // can't I just pass it in the ctor?
        public Func<string, Assembly> AssemblyFileLoader { get; set; }


        //why is this virtual?
        public virtual void LoadAssembliesFromPackage(IPackageInfo packageInfo)
        {
            _currentPackage = packageInfo;
            packageInfo.LoadAssemblies(this);
        }

        public void ReadPackage(IPackageInfo package, IPackageLog log)
        {
            _currentPackage = package;

            //double dispatch - hard to follow - at the moment
            package.LoadAssemblies(this);
        }

        public IList<Assembly> Assemblies
        {
            get { return _assemblies; }
        }

        static Assembly loadPackageAssemblyFromAppBinPath(string file)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(file);
            var appBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath ?? AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (!Path.GetDirectoryName(file).EqualsIgnoreCase(appBinPath))
            {
                var destFileName = appBinPath.AppendPath(Path.GetFileName(file));
                if (shouldUpdateFile(file, destFileName))
                {
                    File.Copy(file, destFileName, true);
                }
            }
            return Assembly.Load(assemblyName);
        }

        static bool shouldUpdateFile(string source, string destination)
        {
            return !File.Exists(destination) || File.GetLastWriteTimeUtc(source) > File.GetLastWriteTimeUtc(destination);
        }

        bool hasAssemblyByName(string assemblyName)
        {
            // I know, packaging *ONLY* supporting one version of a dll.  Use older stuff to 
            // make redirects go
            return (_assemblies.Any(x => x.GetName().Name == assemblyName));
        }

        // need to try to load the assembly by name first!!!
        void IAssemblyRegistration.LoadFromFile(string fileName, string assemblyName)
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

        void IAssemblyRegistration.Use(Assembly assembly)
        {
            if (hasAssemblyByName(assembly.GetName().Name))
            {
                _diagnostics.LogDuplicateAssembly(_currentPackage, assembly.GetName().FullName);
                return;
            }
            
            _diagnostics.LogAssembly(_currentPackage, assembly, DIRECTLY_REGISTERED_MESSAGE);
            _assemblies.Add(assembly);
        }

    }
    
}