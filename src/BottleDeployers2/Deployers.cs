using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BottleDeployers1;
using Bottles.Deployment;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;

namespace BottleDeployers2
{
    public class FourDeployer : StubDeployer<FourDirective> { }
    public class FiveDeployer : StubDeployer<FiveDirective> { }
    public class SixDeployer : StubDeployer<SixDirective> { }

    public class FourDirective : IDirective
    {
        public FourDirective()
        {
            Name = "somebody";
        }

        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class FiveDirective : IDirective
    {
        public string City { get; set; }
        public bool IsDomestic { get; set; }
    }

    public class SixDirective : IDirective
    {
        public int Threshold { get; set; }
        public string Direction { get; set; }
    }

    public class SevenDirective : IDirective{}

    public class SevenFinalizer : IFinalizer<SevenDirective>
    {
        public void Execute(SevenDirective directive, HostManifest host, IPackageLog log)
        {
            throw new NotImplementedException();
        }

        public string GetDescription(SevenDirective directive)
        {
            throw new NotImplementedException();
        }
    }
}
