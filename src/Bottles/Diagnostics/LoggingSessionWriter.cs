using FubuCore;
using HtmlTags;

namespace Bottles.Diagnostics
{
    public static class LoggingSessionWriter
    {
        public static TableTag Write(LoggingSession session)
        {
            var table = new TableTag();
            table.AddClass("table");
            table.AddHeaderRow(r =>
            {
                r.Header("Description");
                r.Header("Provenance");
            });

            session.EachLog((target, log) =>
            {
                table.AddBodyRow(row =>
                {
                    row.Add("td", td =>
                    {
                        td.Append(new HtmlTag("h3").Text("Directive: "+PackagingDiagnostics.GetTypeName(target)));
                        td.Append(new HtmlTag("h5").Text("Desc: " + target.ToString()));
                        var time = new HtmlTag("h6").Text(log.TimeInMilliseconds.ToString()+"ms");
                        td.Append(time);
                    });
                    
                    row.Cell(log.Provenance);
                    
                });

                if (log.FullTraceText().IsNotEmpty())
                {
                    table.AddBodyRow(row =>
                    {
                        row.Cell().Attr("colspan", "2").AddClass("log").Add("pre").Text(log.FullTraceText());
                        if (!log.Success)
                        {
                            row.AddClass("failure");
                        }
                    });
                }
            });

            return table;
        }
    }
}