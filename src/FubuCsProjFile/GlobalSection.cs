using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;

namespace FubuCsProjFile
{
    public class GlobalSection
    {
        private readonly string _declaration;
        private readonly IList<string> _properties = new List<string>();
        private readonly SolutionLoading _order;
        private readonly string _name;

        public GlobalSection(string declaration)
        {
            _declaration = declaration.Trim();
            _order = declaration.Split('=').Last().Trim().ToEnum<SolutionLoading>();
            var start = declaration.IndexOf('(');
            var end = declaration.IndexOf(')');

            _name = declaration.Substring(start + 1, end - start - 1);
        }

        public string Declaration
        {
            get { return _declaration; }
        }

        public string SectionName
        {
            get { return _name; }
        }

        public IList<string> Properties
        {
            get { return _properties; }
        }

        public SolutionLoading LoadingOrder
        {
            get
            {
                return _order;
            }
        }

        public void Read(string text)
        {
            _properties.Add(text.Trim());
        }

        public void Write(StringWriter writer)
        {
            writer.WriteLine("\t" + _declaration);
            _properties.Each(x => writer.WriteLine("\t\t" + x));
            writer.WriteLine("\t" + Solution.EndGlobalSection);
        }
    }
}