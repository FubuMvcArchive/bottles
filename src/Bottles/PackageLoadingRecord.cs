using System;

namespace Bottles
{
    public class PackageLoadingRecord
    {
        public PackageLoadingRecord()
        {
            Started = DateTime.Now;
        }

        public DateTime Started { get; private set; }
        public DateTime Finished { get; set; }

        public override string ToString()
        {
            return string.Format("Bottles Packaging Process finished on {0} at {1}", Finished.ToShortDateString(), Finished.ToLongTimeString());
        }
    }
}