using System;
using System.Collections.Generic;

namespace Bottles.Services
{
    public class BottlesServicePackageFacility : PackageFacility
    {
        private readonly BottleServiceBootstrapper _bootstrapper = new BottleServiceBootstrapper();

        public BottlesServicePackageFacility()
        {
            Bootstrapper(_bootstrapper);
            Loader(new BottleServicePackageLoader());
        }

        public IEnumerable<IBottleService> Services()
        {
            return _bootstrapper.Services();
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