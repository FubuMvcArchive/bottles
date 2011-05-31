﻿using System;
using System.IO;
using Bottles.Exploding;
using Bottles.Zipping;
using FubuCore;
using Topshelf;

namespace Bottles.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            File.WriteAllText("pwd.txt", System.Environment.CurrentDirectory);
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));

            HostFactory.Run(h =>
            {
                h.SetDescription("Bottle Host");
                h.SetServiceName("bottle-host");
                h.SetDisplayName("display");

                h.Service<BottleHost>(c =>
                {
                    c.ConstructUsing(n =>
                    {
                        var fileSystem = new FileSystem();
                        var packageExploder = new PackageExploder(new ZipFileService(fileSystem),
                                                                  new PackageExploderLogger(Console.WriteLine),
                                                                  fileSystem);
                        return new BottleHost(packageExploder, fileSystem);
                    });
                    c.WhenStarted(s => s.Start());
                    c.WhenStopped(s => s.Stop());
                });
            });
        }
    }
}
