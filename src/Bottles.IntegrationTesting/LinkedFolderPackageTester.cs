using System;
using System.IO;
using Bottles.PackageLoaders.LinkedFolders;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.IntegrationTesting
{
    [TestFixture]
    public class LinkedFolderPackageTester : IntegrationTestContext
    {
        [Test]
        public void read_and_write_files_from_a_linked_folder()
        {
            RunBottlesCommand("init bottles-staging BottleProject");

            // REALLY NEED THIS CLEAN TO BE A VALID TEST
            File.Delete("BottleProject.dll");

            AlterManifest(manifest =>
            {
                manifest.RemoveAllAssemblies();
                manifest.AddAssembly("BottleProject");
            });

            Recompile();

            RunBottlesCommand("link {0} {1}".ToFormat(AppDomain.CurrentDomain.BaseDirectory, StagingDirectory));

            _domain.Proxy.LoadViaFolder(StagingDirectory);
            _domain.Proxy.ReadWebContent("content/scripts/script1.js").Trim()
                   .ShouldEqual("var x = 1;");

            _domain.Proxy.ReadData("1.txt").Trim()
                   .ShouldEqual("Original Value");
        }

        [Test]
        public void read_and_write_files_from_a_linked_folder_without_first_initializing_the_linked_folder_will_create_a_default_package_manifest_and_JUST_WORK()
        {
            //RunBottlesCommand("init bottles-staging BottleProject");

            // REALLY NEED THIS CLEAN TO BE A VALID TEST
            File.Delete("BottleProject.dll");
            File.Delete(LinkManifest.FILE);

//            AlterManifest(manifest =>
//            {
//                manifest.RemoveAllAssemblies();
//                manifest.AddAssembly("BottleProject");
//            });

            Recompile();

            RunBottlesCommand("link {0} {1}".ToFormat(AppDomain.CurrentDomain.BaseDirectory, StagingDirectory));

            _domain.Proxy.LoadViaFolder(StagingDirectory);
            _domain.Proxy.ReadWebContent("content/scripts/script1.js").Trim()
                   .ShouldEqual("var x = 1;");

            _domain.Proxy.ReadData("1.txt").Trim()
                   .ShouldEqual("Original Value");
        }

        [Test]
        public void read_and_write_files_from_a_linked_folder_without_first_initializing_the_linked_folder_manifest_will_create_a_default_package_manifest_and_JUST_WORK()
        {
            //RunBottlesCommand("init bottles-staging BottleProject");

            // REALLY NEED THIS CLEAN TO BE A VALID TEST
            File.Delete("BottleProject.dll");
            File.Delete(LinkManifest.FILE);

            //            AlterManifest(manifest =>
            //            {
            //                manifest.RemoveAllAssemblies();
            //                manifest.AddAssembly("BottleProject");
            //            });

            Recompile();

            RunBottlesCommand("link {0} {1}".ToFormat(AppDomain.CurrentDomain.BaseDirectory, StagingDirectory));
            File.Delete(StagingDirectory.AppendPath(PackageManifest.FILE));

            _domain.Proxy.LoadViaFolder(StagingDirectory);
            _domain.Proxy.ReadWebContent("content/scripts/script1.js").Trim()
                   .ShouldEqual("var x = 1;");

            _domain.Proxy.ReadData("1.txt").Trim()
                   .ShouldEqual("Original Value");
        }
    }
}