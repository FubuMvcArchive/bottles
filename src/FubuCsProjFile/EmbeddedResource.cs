namespace FubuCsProjFile
{
    public class EmbeddedResource
    {
        private readonly string _relativePath;
        public static readonly string ItemName = "EmbeddedResource";

        public EmbeddedResource(string relativePath)
        {
            _relativePath = relativePath;
        }

        public string RelativePath
        {
            get { return _relativePath; }
        }

        protected bool Equals(EmbeddedResource other)
        {
            return string.Equals(_relativePath, other._relativePath);
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
            return (_relativePath != null ? _relativePath.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("EmbeddedResource: {0}", _relativePath);
        }
    }
}