using System;
using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles
{
    public class LambdaBootstrapper : IBootstrapper
    {
        private readonly Func<IBottleLog, IEnumerable<IActivator>> _bootstrapper;

        public LambdaBootstrapper(Func<IBottleLog, IEnumerable<IActivator>> bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        public string Provenance { get; set; }

        public IEnumerable<IActivator> Bootstrap(IBottleLog log)
        {
            return _bootstrapper(log);
        }

        public override string ToString()
        {
            return string.Format("Lambda expression at: {0}", Provenance);
        }
    }
}