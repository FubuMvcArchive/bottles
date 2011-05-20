using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.Deployment.Parsing;
using Bottles.Diagnostics;
using FubuCore.Binding;
using FubuCore.Configuration;
using HtmlTags;

namespace Bottles.Deployment.Diagnostics
{
    public class DeploymentReport
    {
        private readonly HtmlDocument _document;

        public DeploymentReport(string title)
        {
            _document = new HtmlDocument
                        {
                            Title = title
                        };

            //_document.AddStyle(getCss());
            _document.Push("div").AddClass("main");

            _document.Add("h2").Text(title);
        }

        public HtmlDocument Document
        {
            get { return _document; }
        }

        private static string getCss()
        {
            var type = typeof(DeploymentReport);
            var stream = type.Assembly.GetManifestResourceStream(type, "diagnostics.css");
            if (stream == null) return String.Empty;
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public void WriteDeploymentPlan(DeploymentPlan plan)
        {
            writeOptions(plan);
            writeEnvironmentSettings(plan);
            writeHostSettings(plan);
        }

        private void writeEnvironmentSettings(DeploymentPlan plan)
        {
            _document.Add("h3").Text("Profile / Environment Substitutions");

            var report = plan.GetSubstitutionDiagnosticReport();

            writeSettings(report);

        }

        private void writeOptions(DeploymentPlan plan)
        {
            var table = new TableTag();
            table.Id("properties");
            table.AddProperty("Written at", DateTime.Now.ToLongTimeString());
            table.AddProperty("Profile", plan.Options.ProfileName); // TODO -- add file name
            table.AddProperty("Recipes", plan.Recipes.Select(x => x.Name).Join(", "));
            table.AddProperty("Hosts", plan.Hosts.Select(x => x.Name).Join(", "));
            table.AddProperty("Bottles", plan.Hosts.SelectMany(host => host.BottleReferences).Select(bottle => bottle.Name).Join(", "));
            _document.Add(table);
        }


        private void writeHostSettings(DeploymentPlan plan)
        {
            _document.Add("h3").Text("Hosts and Directives");
            
            plan.Hosts.Each(writeHostSettings);

        }

        private void writeHostSettings(HostManifest host)
        {
            _document.Add("h4").Text(host.Name);

            var settingDataSources = host.CreateDiagnosticReport();

            writeSettings(settingDataSources);
        }

        private void writeSettings(IEnumerable<SettingDataSource> settingDataSources)
        {
            var table = new TableTag();
            table.AddClass("details");
            table.AddHeaderRow("Key", "Value", "Provenance");

            settingDataSources.Each(s =>
            {
                table.AddBodyRow(s.Key, s.Value, s.Provenance);
            });

            _document.Add(table);
        }

        public void WriteLoggingSession(LoggingSession session)
        {
            _document.Add("h3").Text("Logs");
            var tag = LoggingSessionWriter.Write(session);
            _document.Push(tag);
        }
    }
}