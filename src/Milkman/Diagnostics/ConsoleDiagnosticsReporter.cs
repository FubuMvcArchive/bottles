using System;
using System.Collections.Generic;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime;

namespace Bottles.Deployment.Diagnostics
{
    public class ConsoleDiagnosticsReporter : IDiagnosticsReporter
    {
        public void WriteReport(DeploymentOptions options, DeploymentPlan plan)
        {
            Console.WriteLine("Environment");
            plan.Settings.Environment.Data.AllKeys.Each(key =>
            {
                var value = plan.Settings.Environment.Data[key];
                //province data?
                //resolve substitutions
                Console.WriteLine("{0}={1}", key, value);
            });


            plan.Hosts.Each(
                host =>
                {
                    host.AllSettingsData().Each(
                        s =>
                        {
                            s.AllKeys.Each(
                                key => { Console.WriteLine("{0}:{1}:{2}:{3}", s.Category, s.Provenance, key, s[key]); });
                        });
                });
        }
    }
}