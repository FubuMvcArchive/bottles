using System.Xml;

namespace FubuCsProjFile.MSBuild
{
    public class MSBuildObject
    {
        private readonly XmlElement elem;

        public MSBuildObject(XmlElement elem)
        {
            this.elem = elem;
        }

        public XmlElement Element
        {
            get { return elem; }
        }

        public string Condition
        {
            get { return Element.GetAttribute("Condition"); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Element.RemoveAttribute("Condition");
                else
                    Element.SetAttribute("Condition", value);
            }
        }

        protected XmlElement AddChildElement(string name)
        {
            XmlElement e = elem.OwnerDocument.CreateElement(null, name, MSBuildProject.Schema);
            elem.AppendChild(e);
            return e;
        }
    }
}