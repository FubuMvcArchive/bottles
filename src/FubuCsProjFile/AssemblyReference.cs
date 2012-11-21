using Microsoft.Build.Evaluation;

namespace FubuCsProjFile
{
    public class AssemblyReference
    {
        private readonly string _name;
        private readonly string _hintPath;

        public AssemblyReference(ProjectItem item)
        {
            _name = item.EvaluatedInclude;
            _hintPath = item.GetMetadataValue("HintPath");
        }

        public string Name
        {
            get { return _name; }
        }

        public string HintPath
        {
            get { return _hintPath; }
        }

        protected bool Equals(AssemblyReference other)
        {
            return string.Equals(_name, other._name) && string.Equals(_hintPath, other._hintPath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssemblyReference) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name != null ? _name.GetHashCode() : 0)*397) ^ (_hintPath != null ? _hintPath.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, HintPath: {1}", Name, HintPath);
        }
    }
}