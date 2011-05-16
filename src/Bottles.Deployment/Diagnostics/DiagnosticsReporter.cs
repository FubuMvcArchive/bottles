using System;
using System.Collections.Generic;
using System.IO;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime;
using HtmlTags;

namespace Bottles.Deployment.Diagnostics
{
    public interface IDiagnosticsReporter
    {
        void Report(DeploymentOptions options);
        void Report(DeploymentOptions options, string locationToWriteReport);
    }

    public class DiagnosticsReporter : IDiagnosticsReporter
    {
        private IProfileReader _reader;

        public DiagnosticsReporter(IProfileReader reader)
        {
            _reader = reader;
        }

        public void Report(DeploymentOptions options)
        {
            Report(options, "deploymentplan.html");
        }
        public void Report(DeploymentOptions options, string locationToWriteReport)
        {
            var plan = _reader.Read(options);

            var doc = new HtmlDocument();
            doc.Title = "Bottles Deployment Diagnostics";
            doc.AddStyle(getCss());

            HtmlTag mainDiv = new HtmlTag("div").AddClass("main");
            mainDiv.Add("h2").Text("Bottles Deployment Diagnostics");

            var left = buildLeftside(plan);
            mainDiv.Append(left);


            var right = buildRightside(plan);
            mainDiv.Append(right);

            doc.Add(mainDiv);
            doc.WriteToFile(locationToWriteReport);
        }

        private HtmlTag buildRightside(DeploymentPlan plan)
        {
            var right = new HtmlTag("div").AddClass("right");
            right.Append(new HtmlTag("h3").Text("Settings"));

            plan.Hosts.Each(h =>
            {
                right.Append(new HtmlTag("h4").Text(h.Name));
                var table = new TableTag();
                table.AddClass("details");

                table.AddHeaderRow(row =>
                {
                    row.Cell("Key");
                    row.Cell("Value");
                    row.Cell("Porvenance");
                });

                h.CreateDiagnosticReport().Each(s =>
                {
                    table.AddHeaderRow(row =>
                    {
                        row.Cell(s.Key);
                        row.Cell(s.Value);
                        row.Cell(s.Provenance);
                    });
                });

                right.Append(table);
            });

            return right;
        }

        private HtmlTag buildLeftside(DeploymentPlan dp)
        {
            HtmlTag left = new HtmlTag("div").AddClass("left");
            var tags = new List<HtmlTag>();

            tags.Add(commandLine());
            tags.Add(addRecipes(dp));
            tags.Add(addHosts(dp));
            tags.AddRange(addEnvironment(dp));
            left.Append(tags);

            return left;
        }

        private IList<HtmlTag> addEnvironment(DeploymentPlan dp)
        {
            HtmlTag tag = new HtmlTag("h3").Text("Environment");

            var dl2 = new DLTag();
            HtmlTag x = new HtmlTag("h4").Text("Settings By Host");
            dp.Environment.EnvironmentSettingsData().AllKeys.Each(
                k => { dl2.AddDefinition(k, dp.Environment.EnvironmentSettingsData()[k]); });

            return new List<HtmlTag> {tag, dl2};
        }

        private HtmlTag addHosts(DeploymentPlan dp)
        {
            HtmlTag tag = new HtmlTag("h3").Text("Hosts");

            var ol = new HtmlTag("ol");
            dp.Hosts.Each(h => { ol.Add("li").Text(h.Name); });
            tag.After(ol);

            return tag;
        }

        private HtmlTag addRecipes(DeploymentPlan dp)
        {
            HtmlTag tag = new HtmlTag("h3")
                .Text("Recipes");
            var ul = new HtmlTag("ol");

            dp.Recipes.Each(r => { ul.Add("li").Text(r.Name); });

            tag.After(ul);
            return tag;
        }

        private HtmlTag commandLine()
        {
            var dl = new DLTag();

            dl.AddDefinition("Command Line:", "bottle deploy {profile}");
            dl.AddDefinition("Ran On:", "2pm");
            dl.AddDefinition("Profile:", "{profile name}");

            return dl;
        }

        private string getCss()
        {
            Type type = typeof (DiagnosticsReporter);
            string filename = "diagnostics.css";
            Stream stream = type.Assembly.GetManifestResourceStream(type, filename);
            if (stream == null) return String.Empty;
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}