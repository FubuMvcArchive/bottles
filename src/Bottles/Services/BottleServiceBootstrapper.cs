using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles.Services
{
    public class BottleServiceBootstrapper : IBootstrapper
    {
        private readonly IList<IBottleService> _services = new List<IBottleService>();
 
        public IEnumerable<IActivator> Bootstrap(IPackageLog log)
        {
            _services.Clear();
            _services.AddRange(BottleServiceFinder.Find(PackageRegistry.PackageAssemblies, log));

            return new IActivator[0];
        }

        public IEnumerable<IBottleService> Services()
        {
            return _services;
        } 
    }
}