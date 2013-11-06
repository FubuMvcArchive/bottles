using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Bottles;
using Bottles.Commands;
using Bottles.PackageLoaders.LinkedFolders;
using FubuCore;

namespace RemoteLinkingHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            var manifest = new LinkManifest();
            manifest.AddRemoteLink(new RemoteLink { Folder = "../../../BottleService1", BootstrapperName = "BottleService1.BottleService1Bootstrapper, BottleService1" });
            manifest.AddRemoteLink(new RemoteLink { Folder = "../../../BottleService2", BootstrapperName = "BottleService2.BottleService2Bootstrapper, BottleService2" });
            manifest.AddRemoteLink(new RemoteLink { Folder = "../../../BottleService3", BootstrapperName = "BottleService3.BottleService3Bootstrapper, BottleService3" });

            new FileSystem().WriteObjectToFile(LinkManifest.FILE, manifest);

            PackageRegistry.LoadPackages(x => {
                
            });

            PackageRegistry.AssertNoFailures();

            Console.WriteLine("Type anything to quit");
            Console.ReadLine();

        }
    }
}
