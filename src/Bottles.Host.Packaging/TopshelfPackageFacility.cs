using System;
using System.Collections.Generic;
using FubuCore;

namespace Bottles.Host.Packaging
{
    public class TopshelfPackageFacility : PackageFacility
    {
        public static readonly string TopshelfPackagesFolder = "topshelf-packages";
        public static readonly string TopshelfContentFolder = "topshelf-content";

        public static string GetApplicationPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetExplodedPackagesDirectory()
        {
            return GetApplicationPath().AppendPath(TopshelfContentFolder);
        }

        public static IEnumerable<string> GetPackageDirectories()
        {
            yield return GetApplicationPath().AppendPath(TopshelfPackagesFolder);
            yield return GetApplicationPath().AppendPath(TopshelfContentFolder);
        }
    }
}