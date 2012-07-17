using System;
using System.Collections.Generic;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Environment
{
    public class EnvironmentRunner
    {
        private readonly EnvironmentRun _run;

        public EnvironmentRunner(EnvironmentRun run)
        {
            _run = run;
        }

        public IEnumerable<EnvironmentLogEntry> ExecuteEnvironment(params Action<IInstaller, IBottleLog>[] actions)
        {
            var log = new BottleLog();

            var list = new List<EnvironmentLogEntry>();

            var environment = findEnvironment(list, log);
            
            if (environment != null)
            {
                startTheEnvironment(list, environment, log, actions);
            }

            return list;
        }

        private static void startTheEnvironment(IList<EnvironmentLogEntry> list, IEnvironment environment, IBottleLog log, params Action<IInstaller, IBottleLog>[] actions)
        {          
            try
            {
                var installers = environment.StartUp(log);

                // This needs to happen regardless, but we want these logs put in before
                // logs for the installers, so we don't do it in the finally{}
                addPackagingLogEntries(list);

                executeInstallers(list, installers, actions);
            }
            catch (Exception ex)
            {
                addPackagingLogEntries(list);
                log.MarkFailure(ex.ToString());
            }
            finally
            {
                list.Add(EnvironmentLogEntry.FromPackageLog(environment, log));
                environment.SafeDispose();
            }
        }

        private static void executeInstallers(IList<EnvironmentLogEntry> list, IEnumerable<IInstaller> installers, IEnumerable<Action<IInstaller, IBottleLog>> actions)
        {
            foreach (var action in actions)
            {
                foreach (var installer in installers)
                {
                    var log = new BottleLog();
                    try
                    {
                        action(installer, log);
                    }
                    catch (Exception e)
                    {
                        log.MarkFailure(e.ToString());
                    }

                    list.Add(installer, log);
                }
            }
        }

        private IEnvironment findEnvironment(List<EnvironmentLogEntry> list, IBottleLog log)
        {
            var environmentType = _run.FindEnvironmentType(log);
            if (environmentType == null)
            {
                throw new EnvironmentRunnerException("Unable to find an IEnvironment type");
            }

            IEnvironment environment = null;
            try
            {
                environment = (IEnvironment) Activator.CreateInstance(environmentType);

            }
            catch (Exception e)
            {
                list.Add(new EnvironmentLogEntry
                         {
                             Description = environmentType.FullName,
                             Success = false,
                             TraceText = e.ToString()
                         });
            }

            return environment;
        }

        private static void addPackagingLogEntries(IList<EnvironmentLogEntry> list)
        {
            if (BottleRegistry.Diagnostics != null)
            {
                BottleRegistry.Diagnostics.EachLog((target, log) => list.Add(target, log));
            }
        }
    }
}