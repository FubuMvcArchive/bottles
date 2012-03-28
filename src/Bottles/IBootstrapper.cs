using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles
{
    public interface IBootstrapper
    {
        //TODO: Rename to build activators
        IEnumerable<IActivator> Bootstrap(IPackageLog log);
    }
}