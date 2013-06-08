using System;
using System.Xml;

namespace FubuCsProjFile.MSBuild
{
    public class MSBuildItem : MSBuildObject
    {
        public MSBuildItem(XmlElement elem)
            : base(elem)
        {
        }

        public string Include
        {
            get { return Element.GetAttribute("Include"); }
            set { Element.SetAttribute("Include", value); }
        }

        public string Name
        {
            get { return Element.Name; }
        }

        public bool HasMetadata(string name)
        {
            return Element[name, MSBuildProject.Schema] != null;
        }

        public void SetMetadata(string name, string value)
        {
            SetMetadata(name, value, true);
        }

        public void SetMetadata(string name, string value, bool isLiteral)
        {
            XmlElement elem = Element[name, MSBuildProject.Schema];
            if (elem == null)
            {
                elem = AddChildElement(name);
                Element.AppendChild(elem);
            }
            elem.InnerXml = value;
        }

        public void UnsetMetadata(string name)
        {
            XmlElement elem = Element[name, MSBuildProject.Schema];
            if (elem != null)
            {
                Element.RemoveChild(elem);
                if (!Element.HasChildNodes)
                    Element.IsEmpty = true;
            }
        }

        public string GetMetadata(string name)
        {
            XmlElement elem = Element[name, MSBuildProject.Schema];
            if (elem != null)
                return elem.InnerXml;
            else
                return null;
        }

        public bool GetMetadataIsFalse(string name)
        {
            return String.Compare(GetMetadata(name), "False", StringComparison.OrdinalIgnoreCase) == 0;
        }

        public void MergeFrom(MSBuildItem other)
        {
            foreach (XmlNode node in Element.ChildNodes)
            {
                if (node is XmlElement)
                    SetMetadata(node.LocalName, node.InnerXml);
            }
        }
    }
}