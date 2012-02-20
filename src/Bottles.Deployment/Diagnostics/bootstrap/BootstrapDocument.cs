using System;
using System.IO;
using HtmlTags;
using FubuCore;

namespace Bottles.Deployment.Diagnostics.bootstrap
{
    public class BootstrapDocument : HtmlDocument
    {
        public BootstrapDocument()
        {
            AddStyle(getFile("css.bootstrap.min.css"));
            AddStyle(getFile("css.bootstrap-responsive.min.css"));
            AddJavaScript(getFile("js.bootstrap.js"));

        }



        private static string getFile(string file)
        {
            var type = typeof(BootstrapDocument);
            var stream = type.Assembly.GetManifestResourceStream(type, file);
            if (stream == null) return String.Empty;
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }

    public static class BootstrapHtmlExtensions
    {
        public static void AddPageHeader(this HtmlDocument doc, string text, string small = "")
        {
            var div = new HtmlTag("div").AddClass("page-header");
            var h1 = new HtmlTag("h1").Text(text);

            if (small.IsNotEmpty())
            {
                h1.Append(new HtmlTag("small").Text(small));
            }
            div.Append(h1);

            doc.Add(div);
        }

        public static void AddPageHeader(this HtmlTag tag, string text, string small)
        {
            var div = new HtmlTag("div").AddClass("page-header");
            var h1 = new HtmlTag("h1").Text(text);
            h1.Append(new HtmlTag("small").Text(small));

            div.Append(h1);

            tag.Append(div);
        }
    }
        
}