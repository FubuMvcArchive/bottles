using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FubuCore;

namespace Bottles.Diagnostics
{
    public class BottlingDiagnostics : IBottlingDiagnostics
    {
        private readonly LoggingSession _log;

        public BottlingDiagnostics(LoggingSession log)
        {
            _log = log;
        }

        public void LogPackage(IBottleInfo bottle, IBottleLoader loader)
        {
            _log.LogObject(bottle, "Loaded by " + loader);
            _log.LogFor(loader).AddChild(bottle);
        }

        public void LogBootstrapperRun(IBootstrapper bootstrapper, IEnumerable<IActivator> activators)
        {
            var provenance = "Loaded by Bootstrapper:  " + bootstrapper;
            var bootstrapperLog = _log.LogFor(bootstrapper);

            activators.Each(a =>
            {
                _log.LogObject(a, provenance);
                bootstrapperLog.AddChild(a);
            });
        }

        public void LogAssembly(IBottleInfo bottle, Assembly assembly, string provenance)
        {
            try
            {
                var versionInfo = getVersion(assembly);


                _log.LogObject(assembly, provenance);
                var packageLog = _log.LogFor(bottle);
                packageLog.Trace("Loaded assembly '{0}' v{1}".ToFormat(assembly.GetName().FullName,versionInfo.FileVersion));
                packageLog.AddChild(assembly);
            }
            catch (Exception ex)
            {
                throw new Exception("Trying to log assembly '{0}' in package '{1}' at {2}".ToFormat(assembly.FullName, bottle.Name, assembly.Location), ex);
            }
        }

        private static FileVersionInfo getVersion(Assembly assembly)
        {
            try
            {
                return FileVersionInfo.GetVersionInfo(assembly.Location);
            }
            catch (Exception)
            {
                //grrr
                //blowing up at the moment
                return (FileVersionInfo)Activator.CreateInstance(typeof (FileVersionInfo), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[]{"name"}, null);
            }
        }

        // just in log to package
        public void LogDuplicateAssembly(IBottleInfo bottle, string assemblyName)
        {
            _log.LogFor(bottle).Trace("Assembly '{0}' was ignored because it is already loaded".ToFormat(assemblyName));
        }

        public void LogAssemblyFailure(IBottleInfo bottle, string fileName, Exception exception)
        {
            var log = _log.LogFor(bottle);
            log.MarkFailure(exception);
            log.Trace("Failed to load assembly at '{0}'".ToFormat(fileName));
        }


        public void LogObject(object target, string provenance)
        {
            _log.LogObject(target, provenance);
        }

        public void LogExecution(object target, Action continuation)
        {
            _log.LogExecution(target, continuation);
        }

        public IBottleLog LogFor(object target)
        {
            return _log.LogFor(target);
        }

        public void EachLog(Action<object, BottleLog> action)
        {
            _log.EachLog(action);
        }

        public bool HasErrors()
        {
            return _log.HasErrors();
        }

        // TODO -- think about this little puppy
        public static string GetTypeName(object target)
        {
            if (target is IBootstrapper) return typeof (IBootstrapper).Name;
            if (target is IActivator) return typeof (IActivator).Name;
            if (target is IBottleLoader) return typeof (IBottleLoader).Name;
            if (target is IBottleFacility) return typeof (IBottleFacility).Name;
            if (target is IBottleInfo) return typeof (IBottleInfo).Name;
            if (target is Assembly) return typeof (Assembly).Name;

            return target.GetType().Name;
        }
    }
}