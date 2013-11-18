using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bottles;
using Bottles.Commands;
using FubuCore;

namespace RemoteLinkTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var service1Path =
                Environment.CurrentDirectory.ParentDirectory()
                    .ParentDirectory()
                    .ParentDirectory()
                    .AppendPath("ApplicationLoaderService", "bin", "Debug");

            var service2Path =
                Environment.CurrentDirectory.ParentDirectory()
                    .ParentDirectory()
                    .ParentDirectory()
                    .AppendPath("ApplicationSourceService", "bin", "Debug");

            new LinkCommand().Execute(new LinkInput {AppFolder = Environment.CurrentDirectory,CleanAllFlag = true});

            new LinkCommand().Execute(new LinkInput
            {
                AppFolder = Environment.CurrentDirectory,
                BottleFolder = service1Path,
                RemoteFlag = true
            });

            new LinkCommand().Execute(new LinkInput
            {
                AppFolder = Environment.CurrentDirectory,
                BottleFolder = service2Path,
                RemoteFlag = true
            });

            PackageRegistry.LoadPackages(x => {});

            Console.WriteLine("Press any key to quit");
            Console.ReadLine();

        }
    }
}
