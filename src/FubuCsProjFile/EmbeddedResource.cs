using Microsoft.Build.Evaluation;

namespace FubuCsProjFile
{
    public class EmbeddedResource
    {
        private readonly string _name;

        public EmbeddedResource(ProjectItem item)
        {
            _name = item.EvaluatedInclude;
        }

        public string Name
        {
            get { return _name; }
        }

        protected bool Equals(EmbeddedResource other)
        {
            return string.Equals(_name, other._name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EmbeddedResource) obj);
        }

        public override int GetHashCode()
        {
            return (_name != null ? _name.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("EmbeddedResource: {0}", _name);
        }
    }
}