using System;
using System.Collections.Generic;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Tests.Harness
{
    public class FileWriterActivator : IActivator, IDeactivator
    {
        public static readonly IFileSystem fileSystem = new FileSystem();

        private readonly string _directory;

        public FileWriterActivator(string directory)
        {
            _directory = AppDomain.CurrentDomain.BaseDirectory.AppendPath(directory);
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