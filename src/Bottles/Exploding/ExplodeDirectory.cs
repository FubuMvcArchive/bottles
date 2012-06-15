using Bottles.Diagnostics;

namespace Bottles.Exploding
{
    public class ExplodeDirectory
    {
        public string BottleDirectory { get; set;}
        public string DestinationDirectory { get; set; }
        public IPackageLog Log { get; set; }

        public bool Equals(ExplodeDirectory other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.BottleDirectory, BottleDirectory) && Equals(other.DestinationDirectory, DestinationDirectory);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ExplodeDirectory)) return false;
            return Equals((ExplodeDirectory) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((BottleDirectory != null ? BottleDirectory.GetHashCode() : 0)*397) ^ (DestinationDirectory != null ? DestinationDirectory.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("BottleDirectory: {0}, DestinationDirectory: {1}", BottleDirectory, DestinationDirectory);
        }
    }
}