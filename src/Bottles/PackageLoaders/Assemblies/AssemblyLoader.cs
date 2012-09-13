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

        private readonly IBottlingDiagnostics _diagnostics;
        private IPackageInfo _currentPackage;

        public AssemblyLoader(IBottlingDiagnostics diagnostics)
        {
            Assemblies = new List<Assembly>();
            AssemblyFileLoader = LoadPackageAssemblyFromAppBinPath;
            _diagnostics = diagnostics;
        }


        //why is this a public function?
        //so it can be overridden in tests is one reason
        // ? is it used in fubumvc?
        // can't I just pass it in the ctor?
        public Func<string, Assembly> AssemblyFileLoader { get; set; }


        //why is this virtual? - for testing
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
            var assemblyName = Path.GetFileNameWithoutExtension(file);
            var appBinPath = determineAssemblyPath();

            if (!Path.GetDirectoryName(file).EqualsIgnoreCase(appBinPath))
            {
                

                var destFileName = appBinPath.AppendPath(Path.GetFileName(file));
                if (shouldUpdateFile(file, destFileName))
                {
                    Console.WriteLine("Copying {0} to {1}", file.ToFullPath(), destFileName.ToFullPath());
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