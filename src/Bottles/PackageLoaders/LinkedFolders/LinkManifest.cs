using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using Bottles.Commands;
using FubuCore;

namespace Bottles.PackageLoaders.LinkedFolders
{
    [DebuggerDisplay("{debuggerDisplay()}")]
    [XmlType("links")]
    public class LinkManifest
    {
        public static readonly string FILE = ".links";

        private readonly IList<string> _folders = new List<string>();
        private readonly IList<RemoteLink> _remotes = new List<RemoteLink>();

        [XmlElement("remote")]
        public RemoteLink[] RemoteLinks
        {
            get
            {
                return _remotes.ToArray();
            }
            set
            {
                _remotes.Clear();
                _remotes.AddRange(value);
            }
        }


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
            _remotes.RemoveAll(x => x.Folder.EqualsIgnoreCase(folder));
        }

        public void RemoveAllLinkedFolders()
        {
            _folders.Clear();
            _remotes.Clear();
        }

        private string debuggerDisplay()
        {
            return "{0} linked folders".ToFormat(LinkedFolders.Count());
        }

        public RemoteLink AddRemoteLink(LinkInput input)
        {
            var link = _remotes.FirstOrDefault(x => x.Folder.EqualsIgnoreCase(input.BottleFolder));
            if (link == null)
            {
                link = new RemoteLink {Folder = input.BottleFolder};
                _remotes.Add(link);
            }

            link.BootstrapperName = input.BootstrapperFlag;
            link.ConfigFile = input.ConfigFileFlag;

            return link;
        }
    }

    public class RemoteLink
    {
        [XmlAttribute]
        public string Folder { get; set; }

        [XmlAttribute]
        public string ConfigFile { get; set; }
        
        [XmlAttribute]
        public string BootstrapperName { get; set; }

        public override string ToString()
        {
            var description = "Remote link to " + Folder;
            if (ConfigFile.IsNotEmpty())
            {
                description += ", Config File: " + ConfigFile;
            }

            if (BootstrapperName.IsNotEmpty())
            {
                description += ", BootstrapperName: " + BootstrapperName;
            }

            return description;
        }
    }
}