using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.Deployment.Parsing;
using Bottles.Diagnostics;
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

            _document.AddStyle(getCss());
            _document.AddJavaScript(getJs());

            _document.Push("div").AddClass("main");

            _document.Add("h1").Text(title);
        }

        


        public void WriteDeploymentPlan(DeploymentPlan plan)
        {
            writeOptions(plan);
            writeEnvironmentSettings(plan);
            writeHostSettings(plan);
        }

        private void writeOptions(DeploymentPlan plan)
        {
            wrapInCollapsable("Options", div =>
            {

                var table = new TableTag();
                table.Id("properties");
                table.AddProperty("Written at", DateTime.Now.ToLongTimeString());
                table.AddProperty("Profile", plan.Options.ProfileName + " at " + plan.Options.ProfileFileName); // TODO -- add file name
                table.AddProperty("Recipes", plan.Recipes.Select(x => x.Name).OrderBy(x => x).Join(", "));
                table.AddProperty("Hosts", plan.Hosts.Select(x => x.Name).OrderBy(x => x).Join(", "));
                table.AddProperty("Bottles",
                                  plan.Hosts.SelectMany(host => host.BottleReferences).Select(bottle => bottle.Name).
                                      Distinct().OrderBy(x => x).Join(", "));
                div.Append(table);
            });
        }

        private void wrapInCollapsable(string title, Action<HtmlTag> stuff)
        {
            var id = Guid.NewGuid().ToString();
            _document.Add("h2")
                .Text(title).Attr("onclick", "$('#" + id + "').toggle();");

            var div = new DivTag(id);
            div.Style("display", "none");

            stuff(div);

            _document.Add(div);
        }
        private void writeEnvironmentSettings(DeploymentPlan plan)
        {
            var id = Guid.NewGuid().ToString();
            _document.Add("h2")
                .Text("Profile / Environment Substitutions")
                .Attr("onclick","$('#"+id+"').toggle();");

            var div = new DivTag(id);
            div.Style("display", "none");

            var report = plan.GetSubstitutionDiagnosticReport();

            div.Append(writeSettings(report));

            _document.Add(div);
        }

        private void writeHostSettings(DeploymentPlan plan)
        {
            var id = Guid.NewGuid().ToString();
            _document.Add("h2").Text("Hosts and Directives")
                .Attr("onclick", "$('#" + id + "').toggle();");

            var div = new DivTag(id);
            div.Style("display", "none");

            plan.Hosts.Each(h=>
            {
                div.Append(writeHostSettings(h));
            });

            _document.Add(div);
        }

        private IEnumerable<HtmlTag> writeHostSettings(HostManifest host)
        {
            yield return new HtmlTag("h4").Text(host.Name);
            
            var settingDataSources = host.CreateDiagnosticReport();

            yield return writeSettings(settingDataSources);
        }

        private static HtmlTag writeSettings(IEnumerable<SettingDataSource> settingDataSources)
        {
            var table = new TableTag();
            table.AddClass("details");
            table.AddHeaderRow("Key", "Value", "Provenance");

            settingDataSources.Each(s =>
            {
                table.AddBodyRow(s.Key, s.Value, s.Provenance);
            });

            return table;
        }

        public void WriteLoggingSession(LoggingSession session)
        {
            _document.Add("h3").Text("Logs");
            var tag = LoggingSessionWriter.Write(session);
            _document.Push(tag);
        }

        public void WriteSuccessOrFail(LoggingSession session)
        {
            var tag = _document.Add("div");
            var msg = "SUCCESS";
            tag.AddClass("success");

            if(session.HasErrors())
            {
                tag.RemoveClass("success");
                tag.AddClass("failure");

                msg = "FAIL";
            }

            tag.Add("p").Style("margin", "0px 10px").Text(msg);
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
        private static string getJs()
        {
            var type = typeof(DeploymentReport);
            var stream = type.Assembly.GetManifestResourceStream(type, "jquery-1.6.1.min.js");
            if (stream == null) return String.Empty;
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}