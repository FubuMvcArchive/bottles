using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using FubuCore;

namespace FubuCsProjFile
{
    public class MSBuildProject
    {
        public static MSBuildProject Create(string assemblyName)
        {
            var text = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(MSBuildProject),"Project.txt").ReadAllText();

            text = text.Replace("FUBUPROJECTNAME", assemblyName);

            var project = new MSBuildProject();
            project.doc = new XmlDocument
            {
                PreserveWhitespace = false
            };

            project.doc.LoadXml(text);

            return project;
        }

        public const string Schema = "http://schemas.microsoft.com/developer/msbuild/2003";
        private static XmlNamespaceManager manager;
        private readonly Dictionary<XmlElement, MSBuildObject> elemCache = new Dictionary<XmlElement, MSBuildObject>();
        private Dictionary<string, MSBuildItemGroup> bestGroups;
        private ByteOrderMark bom;
        public XmlDocument doc;

        private bool endsWithEmptyLine;
        private string newLine = Environment.NewLine;

        public MSBuildProject()
        {
            doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.AppendChild(doc.CreateElement(null, "Project", Schema));
        }

        internal static XmlNamespaceManager XmlNamespaceManager
        {
            get
            {
                if (manager == null)
                {
                    manager = new XmlNamespaceManager(new NameTable());
                    manager.AddNamespace("tns", Schema);
                }
                return manager;
            }
        }

        public string DefaultTargets
        {
            get { return doc.DocumentElement.GetAttribute("DefaultTargets"); }
            set { doc.DocumentElement.SetAttribute("DefaultTargets", value); }
        }

        public string ToolsVersion
        {
            get { return doc.DocumentElement.GetAttribute("ToolsVersion"); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    doc.DocumentElement.SetAttribute("ToolsVersion", value);
                else
                    doc.DocumentElement.RemoveAttribute("ToolsVersion");
            }
        }

        public List<string> Imports
        {
            get
            {
                var ims = new List<string>();
                foreach (XmlElement elem in doc.DocumentElement.SelectNodes("tns:Import", XmlNamespaceManager))
                {
                    ims.Add(elem.GetAttribute("Project"));
                }
                return ims;
            }
        }

        public IEnumerable<MSBuildPropertyGroup> PropertyGroups
        {
            get
            {
                foreach (XmlElement elem in doc.DocumentElement.SelectNodes("tns:PropertyGroup", XmlNamespaceManager))
                    yield return GetGroup(elem);
            }
        }

        public IEnumerable<MSBuildItemGroup> ItemGroups
        {
            get
            {
                foreach (XmlElement elem in doc.DocumentElement.SelectNodes("tns:ItemGroup", XmlNamespaceManager))
                    yield return GetItemGroup(elem);
            }
        }

        public void Load(string file)
        {
            using (FileStream fs = File.OpenRead(file))
            {
                var buf = new byte[1024];
                int nread, i;

                if ((nread = fs.Read(buf, 0, buf.Length)) <= 0)
                    return;

                if (ByteOrderMark.TryParse(buf, nread, out bom))
                    i = bom.Length;
                else
                    i = 0;

                do
                {
                    // Read to the first newline to figure out which line endings this file is using
                    while (i < nread)
                    {
                        if (buf[i] == '\r')
                        {
                            newLine = "\r\n";
                            break;
                        }
                        else if (buf[i] == '\n')
                        {
                            newLine = "\n";
                            break;
                        }

                        i++;
                    }

                    if (newLine == null)
                    {
                        if ((nread = fs.Read(buf, 0, buf.Length)) <= 0)
                        {
                            newLine = "\n";
                            break;
                        }

                        i = 0;
                    }
                } while (newLine == null);

                // Check for a blank line at the end
                endsWithEmptyLine = fs.Seek(-1, SeekOrigin.End) > 0 && fs.ReadByte() == '\n';
            }

            // Load the XML document
            doc = new XmlDocument();
            doc.PreserveWhitespace = false;

            // HACK: XmlStreamReader will fail if the file is encoded in UTF-8 but has <?xml version="1.0" encoding="utf-16"?>
            // To work around this, we load the XML content into a string and use XmlDocument.LoadXml() instead.
            string xml = File.ReadAllText(file);

            doc.LoadXml(xml);
        }

        public void Save(string fileName)
        {
            // StringWriter.Encoding always returns UTF16. We need it to return UTF8, so the
            // XmlDocument will write the UTF8 header.
            var sw = new ProjectWriter(bom);
            sw.NewLine = newLine;
            doc.Save(sw);

            string content = sw.ToString();
            if (endsWithEmptyLine && !content.EndsWith(newLine))
                content += newLine;

            new FileSystem().WriteStringToFile(fileName, content);
        }

        public void AddNewImport(string name, string condition)
        {
            XmlElement elem = doc.CreateElement(null, "Import", Schema);
            elem.SetAttribute("Project", name);

            var last = doc.DocumentElement.SelectSingleNode("tns:Import[last()]", XmlNamespaceManager) as XmlElement;
            if (last != null)
                doc.DocumentElement.InsertAfter(elem, last);
            else
                doc.DocumentElement.AppendChild(elem);
        }

        public void RemoveImport(string name)
        {
            var elem =
                (XmlElement)
                doc.DocumentElement.SelectSingleNode("tns:Import[@Project='" + name + "']", XmlNamespaceManager);
            if (elem != null)
                elem.ParentNode.RemoveChild(elem);
            else
                //FIXME: should this actually log an error?
                Console.WriteLine("ppnf:");
        }

        public MSBuildPropertySet GetGlobalPropertyGroup()
        {
            var res = new MSBuildPropertyGroupMerged();
            foreach (MSBuildPropertyGroup grp in PropertyGroups)
            {
                if (grp.Condition.Length == 0)
                    res.Add(grp);
            }
            return res.GroupCount > 0 ? res : null;
        }

        public MSBuildPropertyGroup AddNewPropertyGroup(bool insertAtEnd)
        {
            XmlElement elem = doc.CreateElement(null, "PropertyGroup", Schema);

            if (insertAtEnd)
            {
                var last =
                    doc.DocumentElement.SelectSingleNode("tns:PropertyGroup[last()]", XmlNamespaceManager) as XmlElement;
                if (last != null)
                    doc.DocumentElement.InsertAfter(elem, last);
            }
            else
            {
                var first = doc.DocumentElement.SelectSingleNode("tns:PropertyGroup", XmlNamespaceManager) as XmlElement;
                if (first != null)
                    doc.DocumentElement.InsertBefore(elem, first);
            }

            if (elem.ParentNode == null)
            {
                var first = doc.DocumentElement.SelectSingleNode("tns:ItemGroup", XmlNamespaceManager) as XmlElement;
                if (first != null)
                    doc.DocumentElement.InsertBefore(elem, first);
                else
                    doc.DocumentElement.AppendChild(elem);
            }

            return GetGroup(elem);
        }

        public IEnumerable<MSBuildItem> GetAllItems()
        {
            foreach (XmlElement elem in doc.DocumentElement.SelectNodes("tns:ItemGroup/*", XmlNamespaceManager))
            {
                yield return GetItem(elem);
            }
        }

        public IEnumerable<MSBuildItem> GetAllItems(params string[] names)
        {
            string name = string.Join("|tns:ItemGroup/tns:", names);
            foreach (
                XmlElement elem in doc.DocumentElement.SelectNodes("tns:ItemGroup/tns:" + name, XmlNamespaceManager))
            {
                yield return GetItem(elem);
            }
        }

        public MSBuildItemGroup AddNewItemGroup()
        {
            XmlElement elem = doc.CreateElement(null, "ItemGroup", Schema);
            doc.DocumentElement.AppendChild(elem);
            return GetItemGroup(elem);
        }

        public MSBuildItem AddNewItem(string name, string include)
        {
            MSBuildItemGroup grp = FindBestGroupForItem(name);
            return grp.AddNewItem(name, include);
        }

        private MSBuildItemGroup FindBestGroupForItem(string itemName)
        {
            MSBuildItemGroup group;

            if (bestGroups == null)
                bestGroups = new Dictionary<string, MSBuildItemGroup>();
            else
            {
                if (bestGroups.TryGetValue(itemName, out group))
                    return group;
            }

            foreach (MSBuildItemGroup grp in ItemGroups)
            {
                foreach (MSBuildItem it in grp.Items)
                {
                    if (it.Name == itemName)
                    {
                        bestGroups[itemName] = grp;
                        return grp;
                    }
                }
            }
            group = AddNewItemGroup();
            bestGroups[itemName] = group;
            return group;
        }

        public string GetProjectExtensions(string section)
        {
            var elem =
                doc.DocumentElement.SelectSingleNode("tns:ProjectExtensions/tns:" + section, XmlNamespaceManager) as
                XmlElement;
            if (elem != null)
                return elem.InnerXml;
            else
                return string.Empty;
        }

        public void SetProjectExtensions(string section, string value)
        {
            XmlElement elem = doc.DocumentElement["ProjectExtensions", Schema];
            if (elem == null)
            {
                elem = doc.CreateElement(null, "ProjectExtensions", Schema);
                doc.DocumentElement.AppendChild(elem);
            }
            XmlElement sec = elem[section];
            if (sec == null)
            {
                sec = doc.CreateElement(null, section, Schema);
                elem.AppendChild(sec);
            }
            sec.InnerXml = value;
        }

        public void RemoveProjectExtensions(string section)
        {
            var elem =
                doc.DocumentElement.SelectSingleNode("tns:ProjectExtensions/tns:" + section, XmlNamespaceManager) as
                XmlElement;
            if (elem != null)
            {
                var parent = (XmlElement) elem.ParentNode;
                parent.RemoveChild(elem);
                if (!parent.HasChildNodes)
                    parent.ParentNode.RemoveChild(parent);
            }
        }

        public void RemoveItem(MSBuildItem item)
        {
            elemCache.Remove(item.Element);
            var parent = (XmlElement) item.Element.ParentNode;
            item.Element.ParentNode.RemoveChild(item.Element);
            if (parent.ChildNodes.Count == 0)
            {
                elemCache.Remove(parent);
                parent.ParentNode.RemoveChild(parent);
                bestGroups = null;
            }
        }

        internal MSBuildItem GetItem(XmlElement elem)
        {
            MSBuildObject ob;
            if (elemCache.TryGetValue(elem, out ob))
                return (MSBuildItem) ob;
            var it = new MSBuildItem(elem);
            elemCache[elem] = it;
            return it;
        }

        private MSBuildPropertyGroup GetGroup(XmlElement elem)
        {
            MSBuildObject ob;
            if (elemCache.TryGetValue(elem, out ob))
                return (MSBuildPropertyGroup) ob;
            var it = new MSBuildPropertyGroup(this, elem);
            elemCache[elem] = it;
            return it;
        }

        private MSBuildItemGroup GetItemGroup(XmlElement elem)
        {
            MSBuildObject ob;
            if (elemCache.TryGetValue(elem, out ob))
                return (MSBuildItemGroup) ob;
            var it = new MSBuildItemGroup(this, elem);
            elemCache[elem] = it;
            return it;
        }

        public void RemoveGroup(MSBuildPropertyGroup grp)
        {
            elemCache.Remove(grp.Element);
            grp.Element.ParentNode.RemoveChild(grp.Element);
        }

        private class ProjectWriter : StringWriter
        {
            private readonly Encoding encoding;

            public ProjectWriter(ByteOrderMark bom)
            {
                encoding = bom != null ? Encoding.GetEncoding(bom.Name) : null;
                ByteOrderMark = bom;
            }

            public ByteOrderMark ByteOrderMark { get; private set; }

            public override Encoding Encoding
            {
                get { return encoding ?? Encoding.UTF8; }
            }
        }
    }

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

    public interface MSBuildPropertySet
    {
        IEnumerable<MSBuildProperty> Properties { get; }
        MSBuildProperty GetProperty(string name);
        MSBuildProperty SetPropertyValue(string name, string value, bool preserveExistingCase);
        string GetPropertyValue(string name);
        bool RemoveProperty(string name);
        void RemoveAllProperties();
        void UnMerge(MSBuildPropertySet baseGrp, ISet<string> propertiesToExclude);
    }

    internal class MSBuildPropertyGroupMerged : MSBuildPropertySet
    {
        private readonly List<MSBuildPropertyGroup> groups = new List<MSBuildPropertyGroup>();

        public int GroupCount
        {
            get { return groups.Count; }
        }

        public MSBuildProperty GetProperty(string name)
        {
            // Find property in reverse order, since the last set
            // value is the good one
            for (int n = groups.Count - 1; n >= 0; n--)
            {
                MSBuildPropertyGroup g = groups[n];
                MSBuildProperty p = g.GetProperty(name);
                if (p != null)
                    return p;
            }
            return null;
        }

        public MSBuildProperty SetPropertyValue(string name, string value, bool preserveExistingCase)
        {
            MSBuildProperty p = GetProperty(name);
            if (p != null)
            {
                if (!preserveExistingCase || !string.Equals(value, p.Value, StringComparison.OrdinalIgnoreCase))
                {
                    p.Value = value;
                }
                return p;
            }
            return groups[0].SetPropertyValue(name, value, preserveExistingCase);
        }

        public string GetPropertyValue(string name)
        {
            MSBuildProperty prop = GetProperty(name);
            return prop != null ? prop.Value : null;
        }

        public bool RemoveProperty(string name)
        {
            bool found = false;
            foreach (MSBuildPropertyGroup g in groups)
            {
                if (g.RemoveProperty(name))
                {
                    Prune(g);
                    found = true;
                }
            }
            return found;
        }

        public void RemoveAllProperties()
        {
            foreach (MSBuildPropertyGroup g in groups)
            {
                g.RemoveAllProperties();
                Prune(g);
            }
        }

        public void UnMerge(MSBuildPropertySet baseGrp, ISet<string> propertiesToExclude)
        {
            foreach (MSBuildPropertyGroup g in groups)
            {
                g.UnMerge(baseGrp, propertiesToExclude);
            }
        }

        public IEnumerable<MSBuildProperty> Properties
        {
            get
            {
                foreach (MSBuildPropertyGroup g in groups)
                {
                    foreach (MSBuildProperty p in g.Properties)
                        yield return p;
                }
            }
        }

        public void Add(MSBuildPropertyGroup g)
        {
            groups.Add(g);
        }

        private void Prune(MSBuildPropertyGroup g)
        {
            if (g != groups[0] && !g.Properties.Any())
            {
                // Remove this group since it's now empty
                g.Parent.RemoveGroup(g);
            }
        }
    }

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

    public class MSBuildItemGroup : MSBuildObject
    {
        private readonly MSBuildProject parent;

        internal MSBuildItemGroup(MSBuildProject parent, XmlElement elem)
            : base(elem)
        {
            this.parent = parent;
        }

        public IEnumerable<MSBuildItem> Items
        {
            get
            {
                foreach (XmlNode node in Element.ChildNodes)
                {
                    var elem = node as XmlElement;
                    if (elem != null)
                        yield return parent.GetItem(elem);
                }
            }
        }

        public MSBuildItem AddNewItem(string name, string include)
        {
            XmlElement elem = AddChildElement(name);
            MSBuildItem it = parent.GetItem(elem);
            it.Include = include;
            return it;
        }
    }

    public class ByteOrderMark
    {
        private static readonly ByteOrderMark[] table = new[]
        {
            new ByteOrderMark("UTF-8", new byte[] {0xEF, 0xBB, 0xBF}),
            new ByteOrderMark("UTF-32BE", new byte[] {0x00, 0x00, 0xFE, 0xFF}),
            new ByteOrderMark("UTF-32LE", new byte[] {0xFF, 0xFE, 0x00, 0x00}),
            new ByteOrderMark("UTF-16BE", new byte[] {0xFE, 0xFF}),
            new ByteOrderMark("UTF-16LE", new byte[] {0xFF, 0xFE}),
            new ByteOrderMark("UTF-7", new byte[] {0x2B, 0x2F, 0x76, 0x38}),
            new ByteOrderMark("UTF-7", new byte[] {0x2B, 0x2F, 0x76, 0x39}),
            new ByteOrderMark("UTF-7", new byte[] {0x2B, 0x2F, 0x76, 0x2B}),
            new ByteOrderMark("UTF-7", new byte[] {0x2B, 0x2F, 0x76, 0x2F}),
            new ByteOrderMark("UTF-1", new byte[] {0xF7, 0x64, 0x4C}),
            new ByteOrderMark("UTF-EBCDIC", new byte[] {0xDD, 0x73, 0x66, 0x73}),
            new ByteOrderMark("SCSU", new byte[] {0x0E, 0xFE, 0xFF}),
            new ByteOrderMark("BOCU-1", new byte[] {0xFB, 0xEE, 0x28}),
            new ByteOrderMark("GB18030", new byte[] {0x84, 0x31, 0x95, 0x33}),
        };

        private ByteOrderMark(string name, byte[] bytes)
        {
            Bytes = bytes;
            Name = name;
        }

        public string Name { get; private set; }

        public byte[] Bytes { get; private set; }

        public int Length
        {
            get { return Bytes.Length; }
        }

        public static ByteOrderMark GetByName(string name)
        {
            for (int i = 0; i < table.Length; i++)
            {
                if (table[i].Name == name)
                    return table[i];
            }

            return null;
        }

        public static bool TryParse(byte[] buffer, int available, out ByteOrderMark bom)
        {
            if (buffer.Length >= 2)
            {
                for (int i = 0; i < table.Length; i++)
                {
                    bool matched = true;

                    if (available < table[i].Bytes.Length)
                        continue;

                    for (int j = 0; j < table[i].Bytes.Length; j++)
                    {
                        if (buffer[j] != table[i].Bytes[j])
                        {
                            matched = false;
                            break;
                        }
                    }

                    if (matched)
                    {
                        bom = table[i];
                        return true;
                    }
                }
            }

            bom = null;

            return false;
        }

        public static bool TryParse(Stream stream, out ByteOrderMark bom)
        {
            var buffer = new byte[4];
            int nread;

            if ((nread = stream.Read(buffer, 0, buffer.Length)) < 2)
            {
                bom = null;

                return false;
            }

            return TryParse(buffer, nread, out bom);
        }
    }
}