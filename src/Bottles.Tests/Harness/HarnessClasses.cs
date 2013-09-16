using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bottles.Diagnostics;
using Bottles.Services;
using FubuCore;
using NUnit.Framework;

namespace Bottles.Tests.Harness
{
    public class ServiceRunner
    {
        private readonly string _directory;
        private readonly string[] _serviceDirectories;
        private readonly ServiceFileChecker _checker;

        public ServiceRunner(string directory, params string[] serviceDirectories)
        {
            var target = ".".ToFullPath().Split(Path.DirectorySeparatorChar).Last();
            _directory = ".".ToFullPath()
                .ParentDirectory().ParentDirectory() // project
                .ParentDirectory() // solution
                .AppendPath(directory, "bin", target);

            _checker = new ServiceFileChecker(_directory, serviceDirectories);
        }

        public void Execute()
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "BottleServiceRunner.exe",
                WorkingDirectory = _directory,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var process = Process.Start(processInfo);

            _checker.WaitForActivated();

            process.StandardInput.WriteLine("\x3");
            
            _checker.WaitForDeactivated();

            if (_checker.Messages.Any())
            {
                _checker.Messages.Each(x => Debug.WriteLine(x));
                Assert.Fail("Stuff didn't work, see the above messages");
            }
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


    public class ServiceFileChecker
    {
        private readonly string _rootDirectory;
        private readonly string[] _directories;

        private readonly IList<string> _messages = new List<string>(); 

        public ServiceFileChecker(string rootDirectory, params string[] directories)
        {
            _rootDirectory = rootDirectory;
            _directories = directories;
        }

        private bool exists(string directory, string file)
        {
            return File.Exists(_rootDirectory.AppendPath(directory, file));
        }

        public void WaitForActivated()
        {
            Wait.Until(() => {
                return _directories.All(dir => exists(dir, "activate.txt"));
            }, timeoutInMilliseconds:15000);

            _directories.Where(dir => !exists(dir, "activate.txt"))
                .Each(dir => {
                    _messages.Add("Did not detect an activate.txt file for directory '{0}'".ToFormat(dir));
                });
        }

        public void WaitForDeactivated()
        {
            Wait.Until(() =>
            {
                return _directories.All(dir => exists(dir, "deactivate.txt"));
            }, timeoutInMilliseconds: 15000);

            _directories.Where(dir => !exists(dir, "deactivate.txt"))
                .Each(dir =>
                {
                    _messages.Add("Did not detect an deactivate.txt file for directory '{0}'".ToFormat(dir));
                });
        }

        public IList<string> Messages
        {
            get { return _messages; }
        }
    }
}