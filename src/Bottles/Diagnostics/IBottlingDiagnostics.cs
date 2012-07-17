using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bottles.Diagnostics
{
    public interface IBottlingDiagnostics
    {
        void LogPackage(IBottleInfo bottle, IBottleLoader loader);
        void LogBootstrapperRun(IBootstrapper bootstrapper, IEnumerable<IActivator> activators);
        void LogAssembly(IBottleInfo bottle, Assembly assembly, string provenance);
        void LogDuplicateAssembly(IBottleInfo bottle, string assemblyName);
        void LogAssemblyFailure(IBottleInfo bottle, string fileName, Exception exception);


        //on logging session
        void LogObject(object target, string provenance);
        void LogExecution(object target, Action continuation);
        IBottleLog LogFor(object target);
        void EachLog(Action<object, BottleLog> action);
        bool HasErrors();
    }
}