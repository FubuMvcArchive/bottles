using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using FubuCore;
using System.Linq;

namespace FubuCsProjFile.MSBuild
{
    public class MSBuildProject
    {
        public static MSBuildProject Create(string assemblyName)
        {
            var text = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(MSBuildProject),"Project.txt").ReadAllText();

            text = text
                .Replace("FUBUPROJECTNAME", assemblyName)
                .Replace("GUID", Guid.NewGuid().ToString());
            

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

        public MSBuildItemGroup FindGroup(Func<MSBuildItem, bool> itemTest)
        {
            return ItemGroups.FirstOrDefault(x => x.Items.Any(itemTest))
                   ?? AddNewItemGroup();
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

            ItemGroups.Each(group => {
                var items = group.Items.ToArray();
                group.Element.RemoveAll();

                var orderedItems = items.OrderBy(x => x.Name).ThenBy(x => x.Include);
                orderedItems
                     .Each(item => group.AddNewItem(item.Name, item.Include));
            });


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

        public static MSBuildProject LoadFrom(string fileName)
        {
            var project = new MSBuildProject();
            project.Load(fileName);

            return project;
        }
    }
}