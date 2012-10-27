using System;
using System.Threading;
using Bottles.Exploding;
using Bottles.Host.Packaging;
using FubuCore;

namespace Bottles.Host
{
    class BottleHost
    {
        private IBottleAwareService _svc;
        private readonly IBottleExploder _exploder;
        private readonly IFileSystem _fileSystem;

        public BottleHost(IBottleExploder exploder, IFileSystem fileSystem)
        {
            _exploder = exploder;
            _fileSystem = fileSystem;
        }

        public void Start()
        {
            var manifest = LoadFromFile(ServiceInfo.FILE);

            var type = Type.GetType(manifest.Bootstrapper, true, true);

            //guard clauses here

            _svc = (IBottleAwareService) Activator.CreateInstance(type);

            //this is done so that start can return, as 'LoadPackages' may take some time.
            ThreadPool.QueueUserWorkItem(cb =>
            {
                PackageRegistry.LoadPackages(pkg =>
                {
                    pkg.Loader(new TopshelfPackageLoader(_exploder));

                    pkg.Bootstrapper(_svc);
                });
            });

        }

        public void Stop()
        {
            _svc.Stop();
        }

        ServiceInfo LoadFromFile(string file)
        {
            var si = new ServiceInfo();
            _fileSystem.ReadTextFile(file, s =>
            {
                var bits = s.Split('=');
                if(bits[0]=="Bootstrapper")
                {
                    si.Bootstrapper = bits[1];
                }
                else
                {
                    si.Name = bits[1];
                }
            });
            return si;
        }
    }
}