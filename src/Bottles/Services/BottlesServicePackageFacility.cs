using System;

namespace Bottles.Services
{
    public class BottlesServicePackageFacility : PackageFacility
    {
        private readonly BottleServiceAggregator _aggregator = new BottleServiceAggregator();

        public BottlesServicePackageFacility()
        {
            Bootstrapper(_aggregator);
            Loader(new BottleServicePackageLoader());
        }

        public BottleServiceAggregator Aggregator
        {
            get { return _aggregator; }
        }

        public static string GetApplicationDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public override string ToString()
        {
            return "BottleServicePackageFacility";
        }
    }
}