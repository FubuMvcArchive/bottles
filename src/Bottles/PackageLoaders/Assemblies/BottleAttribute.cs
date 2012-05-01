using System;
using System.Linq;

namespace Bottles.PackageLoaders.Assemblies
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class BottleAttribute : Attribute
    {
        public string Name { get; set; }
        public string[] Assemblies { get; set; }
        public string BinPath { get; set; }
        public string ConfigFileSet { get; set; }
        public string ContentFileSet { get; set; }
        public string DataFileSet { get; set; }
        public string[] Dependencies { get; set; }

        public string Role { get; set; }

        public Dependency[] GetDependencies()
        {
            //todo: need to support required and optional
            return Dependencies.Select(d => Dependency.Optional(d)).ToArray();
        }
    }
}