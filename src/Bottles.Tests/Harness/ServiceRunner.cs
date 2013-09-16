using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
                FileName = _directory.AppendPath("BottleServiceRunner.exe"),
                WorkingDirectory = _directory,
                //RedirectStandardError = true,
                //RedirectStandardInput = true,
                //RedirectStandardOutput = true,
                //CreateNoWindow = true,
                UseShellExecute = false
            };

            var process = Process.Start(processInfo);
            //Console.WriteLine(process.StandardOutput.ReadToEnd());


            _checker.WaitForActivated();

            // need to type CTRL-C here
            
            _checker.WaitForDeactivated();

            if (_checker.Messages.Any())
            {
                _checker.Messages.Each(x => Debug.WriteLine(x));
                Assert.Fail("Stuff didn't work, see the above messages");
            }
        }
    }
}