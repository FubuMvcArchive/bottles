using System;
using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using Bottles.Services;
using FubuCore;

namespace ApplicationLoaderService
{
    public class MyApplicationLoader : IApplicationLoader, IDisposable
    {
        private readonly FileWriterActivator _activator = new FileWriterActivator("loader");

        public IDisposable Load()
        {
            _activator.Activate(new IPackageInfo[0], new PackageLog());

            return this;
        }

        public void Dispose()
        {
            _activator.Deactivate(new PackageLog());
        }
    }

    public class FileWriterActivator : IActivator, IDeactivator
    {
        public static readonly IFileSystem fileSystem = new FileSystem();

        private readonly string _directory;

        public FileWriterActivator(string directory)
        {
            _directory = directory;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            fileSystem.CreateDirectory(_directory);
            fileSystem.WriteStringToFile(_directory.AppendPath("activate.txt"), DateTime.UtcNow.ToString());
        }

        public void Deactivate(IPackageLog log)
        {
            fileSystem.CreateDirectory(_directory);
            fileSystem.WriteStringToFile(_directory.AppendPath("deactivate.txt"), DateTime.UtcNow.ToString());
        }
    }
}