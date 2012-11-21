using System;
using System.Linq;
using System.Reflection;
using Bottles.PackageLoaders.Assemblies;
using Bottles.PackageLoaders.LinkedFolders;
using FubuCore;

namespace Bottles.Tests.IntegrationTesting
{
    public class BottleDomainProxy : MarshalByRefObject
    {
        private IPackageInfo bottle
        {
            get { return PackageRegistry.Packages.Single(x => x.Name == "BottleProject"); }
        }

        public string ReadData(string path)
        {
            return readContent(path, BottleFiles.DataFolder);
        }

        public string ReadWebContent(string path)
        {
            return readContent(path, BottleFiles.WebContentFolder);
        }

        private string readContent(string path, string folderName)
        {
            string returnValue = null;

            bottle.ForFolder(folderName, folder => {
                var file = folder.AppendPath(path);
                returnValue = new FileSystem().ReadStringFromFile(file);
            });

            return returnValue;
        }

        public void LoadViaZip(string folder)
        {
            

            PackageRegistry.LoadPackages(x => {
                x.Loader(new ZipFilePackageLoader(IntegrationTestContext.SolutionDirectory.AppendPath("exploded"), new string[]{folder}));
            });
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void LoadViaAssembly()
        {
            var file = new FileSystem().FindFiles(IntegrationTestContext.StagingDirectory.AppendPath("bin"),
                                                  new FileSet {DeepSearch = true, Include = "*.dll"}).Single();



            var assembly = Assembly.LoadFile(file);

            PackageRegistry.LoadPackages(x => {
                x.Loader(new AssemblyPackageLoader(assembly));
            });
        }

        public void LoadViaFolder(string stagingDirectory)
        {
            PackageRegistry.LoadPackages(x => {
                x.Loader(new LinkedFolderPackageLoader(AppDomain.CurrentDomain.BaseDirectory, folder => folder));
            });
        }
    }
}