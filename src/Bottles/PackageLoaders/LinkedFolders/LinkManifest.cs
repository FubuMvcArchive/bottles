using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using FubuCore;

namespace Bottles.PackageLoaders.LinkedFolders
{
    [DebuggerDisplay("{debuggerDisplay()}")]
    [XmlType("links")]
    public class LinkManifest
    {
        public static readonly string FILE = ".links";

        private readonly IList<string> _folders = new List<string>();

        [XmlElement("include")]
        public string[] LinkedFolders
        {
            get
            {
                return _folders.ToArray();
            }
            set
            {
                _folders.Clear();
                if (value != null) _folders.AddRange(value);
            }
        }

        public bool AddLink(string folder)
        {
            if (_folders.Contains(folder))
            {
                return false;
            }

            _folders.Add(folder);
            return true;
        }

        public void RemoveLink(string folder)
        {
            _folders.Remove(folder);
        }

        public void RemoveAllLinkedFolders()
        {
            _folders.Clear();
        }

        private string debuggerDisplay()
        {
            return "{0} linked folders".ToFormat(LinkedFolders.Count());
        }
    }
}