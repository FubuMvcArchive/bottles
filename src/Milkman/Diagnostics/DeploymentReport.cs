using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.Deployment.Diagnostics.bootstrap;
using Bottles.Deployment.Parsing;
using Bottles.Diagnostics;
using FubuCore.Configuration;
using HtmlTags;

namespace Bottles.Deployment.Diagnostics
{
    public class DeploymentReport
    {
        private readonly BootstrapDocument _document;

        public DeploymentReport(string title)
        {
            _document = new BootstrapDocument();
            _document.Title = title;

            
            _document.AddStyle(getFile("diagnostics.css"));
            
            _document.AddJavaScript(getFile("jquery-1.6.1.min.js"));
            _document.AddJavaScript(getFile("diagnostics.js"));


            //write menu?

            _document.Push("div").AddClass("container");

            _document.AddPageHeader(title, "via Milkman");
        }

        


        public void WriteDeploymentPlan(DeploymentPlan plan)
        {
            writeOptions(plan);
            writeEnvironmentSettings(plan);
            writeHostSettings(plan);
        }

        private void writeOptions(DeploymentPlan plan)
        {

            wrapInSection("Options","Options used at runtime", div =>
            {

                var table = new TableTag();
                table.AddClass("table");
                table.AddProperty("Written at", DateTime.Now.ToLongTimeString());
                table.AddProperty("Profile", plan.Options.ProfileName);

                table.AddBodyRow(tr =>
                {
                    tr.Add("th", th =>
                    {
                        th.Text("Recipes").Append(new HtmlTag("small").Text(" (in order)"));
                    });
                    tr.Add("td", td =>
                    {
                        td.Text(plan.Recipes.Select(x => x.Name).Join(" > "));
                    });
                });

                table.AddProperty("Hosts", plan.Hosts.Select(x => x.Name).OrderBy(x => x).Join(", "));
                table.AddProperty("Bottles",
                                  plan.Hosts.SelectMany(host => host.BottleReferences).Select(bottle => bottle.Name).
                                      Distinct().OrderBy(x => x).Join(", "));
                div.Append(table);
            });
        }
        private void writeEnvironmentSettings(DeploymentPlan plan)
        {
            wrapInSection("Settings", "Profile / Environment Substitutions", div =>
            {
                var report = plan.GetSubstitutionDiagnosticReport();

                div.Append(writeSettings(findProvenanceRoot(), report));
            });
        }
        private void writeHostSettings(DeploymentPlan plan)
        {
            var provRoot = findProvenanceRoot();
            wrapInSection("Directive Values","by host", div =>
            {
                plan.Hosts.Each(h =>
                {
                    div.Append(writeHostSettings(provRoot, h));
                });
            });
        }

        private IEnumerable<HtmlTag> writeHostSettings(string provRoot, HostManifest host)
        {
            yield return new HtmlTag("h4").Text(host.Name);
            
            var settingDataSources = host.CreateDiagnosticReport();

            yield return writeSettings(provRoot, settingDataSources);
        }
        private string findProvenanceRoot()
        {
            return System.Environment.CurrentDirectory;
        }

        private void wrapInSection(string title, string subtitle, Action<HtmlTag> stuff)
        {
            var section = new HtmlTag("section");
            section.AddPageHeader(title, subtitle);

            stuff(section);

            _document.Add(section);
        }



        private static HtmlTag writeSettings(string provRoot ,IEnumerable<SettingDataSource> settingDataSources)
        {
            var table = new TableTag();
            table.AddClass("table");
            table.AddHeaderRow("Key", "Value", "Provenance");

            settingDataSources.Each(s =>
            {
                var prov = s.Provenance.Replace(provRoot, "");
                table.AddBodyRow(s.Key, s.Value, s.Provenance);
            });

            return table;
        }

        public void WriteLoggingSession(LoggingSession session)
        {
            wrapInSection("Logs", "yum", div =>
            {
                var tag = LoggingSessionWriter.Write(session);
                tag.AddClass("details");
                div.Append(tag);
            });
        }

        public void WriteSuccessOrFail(LoggingSession session)
        {
            var tag = _document.Add("div");
            var msg = "SUCCESS";
            tag.AddClass("alert");
            tag.AddClass("alert-success");

            if(session.HasErrors())
            {
                tag.RemoveClass("success");
                tag.RemoveClass("alert-success");

                msg = "FAIL";
            }

            tag.Text(msg);
        }


        public HtmlDocument Document
        {
            get { return _document; }
        }

        private static string getFile(string name)
        {
            var type = typeof(DeploymentReport);
            var stream = type.Assembly.GetManifestResourceStream(type, name);
            if (stream == null) return String.Empty;
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }

    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream stream)
        {
            var buffer = new Byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}