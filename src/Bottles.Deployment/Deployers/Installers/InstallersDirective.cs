using System;
using Bottles.Deployment.Commands;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployment.Deployers.Installers
{
    public class InstallersDirective : IDirective
    {
        private readonly InstallInput _input = new InstallInput();

        public string AppDirectory
        {
            get { return _input.AppFolder; }
            set { _input.AppFolder = value; }
        }

        public InstallMode Mode
        {
            get
            {
                return _input.ModeFlag;
            }
            set { _input.ModeFlag = value; }
        }

        public string LogFile
        {
            get
            {
                return _input.LogFileFlag;
            }
            set
            {
                _input.LogFileFlag = value;
            }
        }

        public InstallInput Input
        {
            get { return _input; }
        }

        public string EnvironmentClassName
        {
            get
            {
                return _input.EnvironmentClassNameFlag;
            }
            set
            {
                _input.EnvironmentClassNameFlag = value == string.Empty ? null : value;
            }
        }

        public string EnvironmentAssembly
        {
            get
            {
                return _input.EnvironmentAssemblyFlag;
            }
            set
            {
                _input.EnvironmentAssemblyFlag = value == string.Empty ? null : value;
            }
        }
    }

    public class InstallersDeployer : IDeployer<InstallersDirective>
    {
        public void Execute(InstallersDirective directive, HostManifest host, IPackageLog log)
        {
            log.Trace(directive.Input.Title());
            log.Trace("Writing the installer log file to " + directive.Input.LogFileFlag.ToFullPath());
            new InstallCommand().Execute(directive.Input);
        }
    }
}