using System.Collections.Generic;
using System.Threading;
using Bottles.Diagnostics;
using FubuCore.Util;

namespace Bottles.Tests
{
    public class StubBottleLoader : IBottleLoader
    {
        private readonly Cache<string, StubBottle> _packages = new Cache<string, StubBottle>(name => new StubBottle(new PackageManifest(){Name = name}));

        public StubBottleLoader(params string[] names)
        {
            names.Each(x => _packages.FillDefault(x));
        }

        public IEnumerable<IBottleInfo> Load(IBottleLog log)
        {
            Thread.Sleep(101);
            return _packages;
        }

        public StubBottle PackageFor(string name)
        {
            return _packages[name];
        }

        public void HasPackage(string name)
        {
            _packages.FillDefault(name);
        }

        public IEnumerable<IBottleInfo> Packages
        {
            get { return _packages; }
        }
    }
}