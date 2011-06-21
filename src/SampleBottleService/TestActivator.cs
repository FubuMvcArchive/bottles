using System;
using System.Collections.Generic;
using System.Linq;
using Bottles;
using Bottles.Diagnostics;
using FubuCore.CommandLine;

namespace SampleBottleService
{
    public class TestActivator :
        IActivator
    {
        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            ConsoleWriter.Write("hi, poopy pants");

            packages.Select(p => p.Name)
                .Each(s => ConsoleWriter.Write(s));
        }
    }
}