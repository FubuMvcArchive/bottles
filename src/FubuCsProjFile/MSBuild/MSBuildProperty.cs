using System.Xml;

namespace FubuCsProjFile.MSBuild
{
    public class MSBuildProperty : MSBuildObject
    {
        public MSBuildProperty(XmlElement elem)
            : base(elem)
        {
        }

        public string Name
        {
            get { return Element.Name; }
        }

        public string Value
        {
            get { return Element.InnerXml; }
            set { Element.InnerXml = value; }
        }
    }
}