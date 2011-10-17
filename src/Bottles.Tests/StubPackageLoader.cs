using System.Collections.Generic;
using System.Threading;
using Bottles.Diagnostics;
using FubuCore.Util;

namespace Bottles.Tests
{
    public class StubPackageLoader : IPackageLoader
    {
        private readonly Cache<string, StubPackage> _packages = new Cache<string, StubPackage>(name => new StubPackage(name));

        public StubPackageLoader(params string[] names)
        {
            names.Each(x => _packages.FillDefault(x));
        }

        public IEnumerable<IPackageInfo> Load(IPackageLog log)
        {
            Thread.Sleep(101);
            return _packages;
        }

        public StubPackage PackageFor(string name)
        {
            return _packages[name];
        }

        public void HasPackage(string name)
        {
            _packages.FillDefault(name);
        }

        public IEnumerable<IPackageInfo> Packages
        {
            get { return _packages; }
        }
    }
}