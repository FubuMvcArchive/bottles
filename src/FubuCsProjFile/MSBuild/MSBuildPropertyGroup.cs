using System;
using System.Collections.Generic;
using System.Xml;

namespace FubuCsProjFile.MSBuild
{
    public class MSBuildPropertyGroup : MSBuildObject, MSBuildPropertySet
    {
        private readonly MSBuildProject parent;
        private readonly Dictionary<string, MSBuildProperty> properties = new Dictionary<string, MSBuildProperty>();

        public MSBuildPropertyGroup(MSBuildProject parent, XmlElement elem)
            : base(elem)
        {
            this.parent = parent;
        }

        public MSBuildProject Parent
        {
            get { return parent; }
        }

        public MSBuildProperty GetProperty(string name)
        {
            MSBuildProperty prop;
            if (properties.TryGetValue(name, out prop))
                return prop;
            XmlElement propElem = Element[name, MSBuildProject.Schema];
            if (propElem != null)
            {
                prop = new MSBuildProperty(propElem);
                properties[name] = prop;
                return prop;
            }
            else
                return null;
        }

        public IEnumerable<MSBuildProperty> Properties
        {
            get
            {
                foreach (XmlNode node in Element.ChildNodes)
                {
                    var pelem = node as XmlElement;
                    if (pelem == null)
                        continue;
                    MSBuildProperty prop;
                    if (properties.TryGetValue(pelem.Name, out prop))
                        yield return prop;
                    else
                    {
                        prop = new MSBuildProperty(pelem);
                        properties[pelem.Name] = prop;
                        yield return prop;
                    }
                }
            }
        }

        public MSBuildProperty SetPropertyValue(string name, string value, bool preserveExistingCase)
        {
            MSBuildProperty prop = GetProperty(name);
            if (prop == null)
            {
                XmlElement pelem = AddChildElement(name);
                prop = new MSBuildProperty(pelem);
                properties[name] = prop;
                prop.Value = value;
            }
            else if (!preserveExistingCase || !string.Equals(value, prop.Value, StringComparison.OrdinalIgnoreCase))
            {
                prop.Value = value;
            }
            return prop;
        }

        public string GetPropertyValue(string name)
        {
            MSBuildProperty prop = GetProperty(name);
            if (prop == null)
                return null;
            else
                return prop.Value;
        }

        public bool RemoveProperty(string name)
        {
            MSBuildProperty prop = GetProperty(name);
            if (prop != null)
            {
                properties.Remove(name);
                Element.RemoveChild(prop.Element);
                return true;
            }
            return false;
        }

        public void RemoveAllProperties()
        {
            var toDelete = new List<XmlNode>();
            foreach (XmlNode node in Element.ChildNodes)
            {
                if (node is XmlElement)
                    toDelete.Add(node);
            }
            foreach (XmlNode node in toDelete)
                Element.RemoveChild(node);
            properties.Clear();
        }

        public void UnMerge(MSBuildPropertySet baseGrp, ISet<string> propsToExclude)
        {
            foreach (MSBuildProperty prop in baseGrp.Properties)
            {
                if (propsToExclude != null && propsToExclude.Contains(prop.Name))
                    continue;
                MSBuildProperty thisProp = GetProperty(prop.Name);
                if (thisProp != null && prop.Value.Equals(thisProp.Value, StringComparison.CurrentCultureIgnoreCase))
                    RemoveProperty(prop.Name);
            }
        }

        public override string ToString()
        {
            string s = "[MSBuildPropertyGroup:";
            foreach (MSBuildProperty prop in Properties)
                s += " " + prop.Name + "=" + prop.Value;
            return s + "]";
        }
    }
}