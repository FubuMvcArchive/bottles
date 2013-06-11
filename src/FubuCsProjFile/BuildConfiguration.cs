using System.IO;
using System.Linq;
using FubuCore;

namespace FubuCsProjFile
{
    public class BuildConfiguration
    {
        public BuildConfiguration()
        {
        }

        public BuildConfiguration(string text)
        {
            var parts = text.Trim().ToDelimitedArray('=');
            Key = parts.First();
            Value = parts.Last();
        }

        public string Key { get; set; }
        public string Value { get; set; }

        protected bool Equals(BuildConfiguration other)
        {
            return string.Equals(Key, other.Key) && string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BuildConfiguration) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null ? Key.GetHashCode() : 0)*397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("Key: {0}, Value: {1}", Key, Value);
        }
    }
}