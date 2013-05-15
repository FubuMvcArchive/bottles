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
        IAssemblyRegistration 
    {
        public static readonly string DIRECTLY_REGISTERED_MESSAGE = "Directly loaded by the Package";

        private readonly IBottlingDiagnostics _diagnostics;
        private IPackageInfo _currentPackage;

        public AssemblyLoader(IBottlingDiagnostics diagnostics)
        {
            Assemblies = new List<Assembly>();
            AssemblyFileLoader = LoadPackageAssemblyFromAppBinPath;
            _diagnostics = diagnostics;
        }

        public Func<string, Assembly> AssemblyFileLoader { get; set; }


        public virtual void LoadAssembliesFromPackage(IPackageInfo packageInfo)
        {
            _currentPackage = packageInfo;
            packageInfo.LoadAssemblies(this);
        }

        public void ReadPackage(IPackageInfo package, IPackageLog log)
        {
            _currentPackage = package;

            package.LoadAssemblies(this);
        }

        public IList<Assembly> Assemblies { get; private set; }

        private static string determineAssemblyPath()
        {
            var privateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
            var applicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (privateBinPath.IsEmpty())
            {
                return applicationBase;
            }

            if (Path.IsPathRooted(privateBinPath))
            {
                return privateBinPath;
            }

            return applicationBase.AppendPath(privateBinPath);
        }

        public static Assembly LoadPackageAssemblyFromAppBinPath(string file)
        {
            // First try to load it locally just in case it's already there
            var assemblyName = Path.GetFileNameWithoutExtension(file);
            var filename = Path.GetFileName(file);
            var path = determineAssemblyPath().AppendPath(filename);
            if (File.Exists(path))
            {
                return Assembly.Load(assemblyName);
            }

            return Assembly.LoadFile(file);
        }

        bool hasAssemblyByName(string assemblyName)
        {
            return (Assemblies.Any(x => x.GetName().Name == assemblyName));
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

                    Assemblies.Add(assembly);
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
            Assemblies.Add(assembly);
        }

    }
    
}