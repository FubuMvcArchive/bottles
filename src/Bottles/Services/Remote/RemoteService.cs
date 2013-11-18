using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            _serviceDirectory = Path.IsPathRooted(_link.Folder)
                ? _link.Folder
                : PackageRegistry.GetApplicationDirectory().AppendPath(_link.Folder);
        }

        public static IEnumerable<RemoteService> LoadLinkedRemotes()
        {
            var manifestFile = PackageRegistry.GetApplicationDirectory().AppendPath(LinkManifest.FILE);
            if (File.Exists(manifestFile))
            {
                return
                    new FileSystem().LoadFromFile<LinkManifest>(manifestFile)
                        .RemoteLinks.Select(x => new RemoteService(x)).ToArray();
            }

            return new RemoteService[0];
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
                        x.Setup.ApplicationName = Path.GetDirectoryName(_serviceDirectory).Replace(" ", "-");
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

        public override string ToString()
        {
            return string.Format("Remote Service at {0}", _serviceDirectory);
        }
    }
}