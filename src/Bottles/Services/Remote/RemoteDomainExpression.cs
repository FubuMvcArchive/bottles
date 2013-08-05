using System;
using System.Collections.Generic;
using System.IO;
using Bottles.Services.Messaging;
using FubuCore;

namespace Bottles.Services.Remote
{
    public interface IAssemblyMover
    {
        void MoveAssemblies(AppDomainSetup setup);
    }

    public class RemoteDomainExpression : IAssemblyMover
    {
        private readonly static FileSystem fileSystem = new FileSystem();
        private readonly MessagingHub _listeners = new MessagingHub();
        private readonly IList<AssemblyRequirement> _requirements = new List<AssemblyRequirement>(); 

        private readonly AppDomainSetup _setup = new AppDomainSetup
        {
            ApplicationName = "Bottle-Services-AppDomain",
            ShadowCopyFiles = "true",
            ConfigurationFile = "BottleServiceRunner.exe.config",
            ApplicationBase = ".".ToFullPath()
        };

        public AppDomainSetup Setup
        {
            get { return _setup; }
        }

        public string ServiceDirectory
        {
            get { return _setup.ApplicationBase; }
            set
            {
                _setup.ApplicationBase = value;

                if (fileSystem.DirectoryExists(value, "bin"))
                {
                    _setup.PrivateBinPath = "bin";
                }

                if (fileSystem.DirectoryExists(value, "bin", "Debug"))
                {
                    _setup.PrivateBinPath = "bin".AppendPath("Debug");
                }


            }
        }

        /// <summary>
        /// Use to force the selection of bin/Debug or bin/Release for the private bin path of the remote AppDomain
        /// </summary>
        public string BuildProfile
        {
            set
            {
                if (fileSystem.DirectoryExists(_setup.ApplicationBase, "bin", value))
                {
                    _setup.PrivateBinPath = "bin".AppendPath(value);
                }
            }
        }

        public MessagingHub Listeners
        {
            get { return _listeners; }
        }

        public string BootstrapperName { get; set; }

        /// <summary>
        /// This is primarily used in development and testing scenarios to remotely run
        /// a service in a parallel project.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="compileTarget"></param>
        public void LoadAssemblyContainingType<T>(string compileTarget = "Debug")
        {
            string assemblyName = typeof (T).Assembly.GetName().Name;
            string domainPath = AppDomain.CurrentDomain.BaseDirectory.ParentDirectory().ParentDirectory().ParentDirectory()
                                   .AppendPath(assemblyName, "bin", compileTarget);

            _setup.ApplicationBase = domainPath;
        }

        public void RequireAssembly(string name)
        {
            _requirements.Add(new AssemblyRequirement(name));
        }

        public void RequireAssemblyContainingType<T>()
        {
            _requirements.Add(new AssemblyRequirement(typeof(T).Assembly));
        }

        void IAssemblyMover.MoveAssemblies(AppDomainSetup setup)
        {
            var binaryPath = setup.ApplicationBase;
            if (setup.PrivateBinPath.IsNotEmpty())
            {
                if (Path.IsPathRooted(setup.PrivateBinPath))
                {
                    binaryPath = setup.PrivateBinPath;
                }
                else
                {
                    binaryPath = setup.ApplicationBase.AppendPath(setup.PrivateBinPath);
                }
            }

            _requirements.Each(x => x.Move(binaryPath));
        }
    }
}