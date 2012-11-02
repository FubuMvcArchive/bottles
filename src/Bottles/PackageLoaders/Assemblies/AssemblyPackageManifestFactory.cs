using System.Linq;
using System.Reflection;
using Bottles.Manifest;
using FubuCore;

namespace Bottles.PackageLoaders.Assemblies
{
    public static class AssemblyPackageManifestFactory
    {
        public static PackageManifest Extract(Assembly assembly)
        {
            var result = extractFromEmbeddedResource(assembly);

            if(result == null)
            {
                result = extractFromAssemblyAttributes(assembly);
                if(result == null)
                {
                    return defaults(assembly);
                }
            }

            return result;
        }


        static PackageManifest extractFromEmbeddedResource(Assembly assembly)
        {
            var resourceName = assembly.GetName().Name + "." + PackageManifest.FILE;

            using (var resource = assembly.GetManifestResourceStream(resourceName))
            {
                if (resource != null)
                {
                    return new PackageManifestReader(new FileSystem(), s => s).LoadFromStream(resource);
                }
            }

            return null;
        }

        static PackageManifest extractFromAssemblyAttributes(Assembly assembly)
        {
            var attribs = assembly.GetCustomAttributes(typeof(BottleAttribute), false);
            if (attribs.Any())
            {
                var attrib = attribs.Single().As<BottleAttribute>();
                return new PackageManifest()
                {
                    Name = attrib.Name,
                    Assemblies = attrib.Assemblies,
                    BinPath = attrib.BinPath,
                    ConfigFileSet = new FileSet()
                    {
                        Include = attrib.ConfigFileSet
                    },
                    ContentFileSet = new FileSet()
                    {
                        Include = attrib.ContentFileSet
                    },
                    DataFileSet = new FileSet()
                    {
                        Include = attrib.DataFileSet
                    },
                    Dependencies = attrib.GetDependencies(),
                    ManifestFileName = "assembly attributes",
                    Role = attrib.Role
                };
            }

            return null;
        }

        static PackageManifest defaults(Assembly assembly)
        {
            return new PackageManifest
            {
                Name = assembly.GetName().Name,
                Role = BottleRoles.Binaries,
                Assemblies = new[] { assembly.FullName },
                Dependencies = new Dependency[0]
            };
        }
    }
}