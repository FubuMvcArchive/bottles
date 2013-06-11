using FubuCsProjFile.MSBuild;
using System.Linq;

namespace FubuCsProjFile
{
    

    public abstract class ProjectItem
    {
        private readonly string _name;

        protected ProjectItem(string name)
        {
            _name = name;
        }

        protected ProjectItem(string name, string include)
        {
            _name = name;
            Include = include;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Include { get; set; }

        internal bool Matches(MSBuildItem item)
        {
            return item.Name == Name && item.Include == Include;
        }

        internal virtual MSBuildItem Configure(MSBuildItemGroup @group)
        {
            var item = @group.Items.FirstOrDefault(Matches)
                       ?? @group.AddNewItem(Name, Include);

            return item;
        }

        internal virtual void Read(MSBuildItem item)
        {
            Include = item.Include;
        }

        protected bool Equals(ProjectItem other)
        {
            return string.Equals(_name, other._name) && string.Equals(Include, other.Include);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProjectItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name != null ? _name.GetHashCode() : 0)*397) ^ (Include != null ? Include.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("Item {0}: {1}", Name, Include);
        }
    }
}