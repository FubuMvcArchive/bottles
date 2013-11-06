using System;
using System.Threading.Tasks;
using Bottles.PackageLoaders.LinkedFolders;
using FubuCore;

namespace Bottles.Services.Remote
{
    public class RemoteService
    {
        private readonly RemoteLink _link;
        private RemoteServiceRunner _runner;
        private readonly AppDomainFileChangeWatcher _watcher;
        private readonly string _serviceDirectory;

        public RemoteService(RemoteLink link)
        {
            _link = link;
            _watcher = new AppDomainFileChangeWatcher(Recycle);
            _serviceDirectory = _link.Folder.ToFullPath();
        }

        public void Recycle()
        {
            Console.WriteLine("Detected changes for the remote service at " + _serviceDirectory);
            _runner.Recycle();
            Console.WriteLine("Successfully recycled the remote service at " + _serviceDirectory);
        }

        public Task Start()
        {
            return Task.Factory.StartNew(() => {
                try
                {
                    _runner = new RemoteServiceRunner(x => {
                    
                        x.ServiceDirectory = _serviceDirectory;
                        if (_link.ConfigFile.IsNotEmpty()) x.Setup.ConfigurationFile = _link.ConfigFile;
                        if (_link.BootstrapperName.IsNotEmpty()) x.BootstrapperName = _link.BootstrapperName;
                        x.Setup.ShadowCopyFiles = true.ToString();
                    });

                    Console.WriteLine("Successfully started the remote service at " + _serviceDirectory);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                _watcher.WatchBinariesAt(_serviceDirectory);
            });
        }

        public string ServiceDirectory
        {
            get { return _serviceDirectory; }
        }

        public RemoteServiceRunner Runner
        {
            get { return _runner; }
        }
    }
}