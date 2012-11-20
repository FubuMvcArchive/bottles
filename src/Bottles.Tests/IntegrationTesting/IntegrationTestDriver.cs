using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Bottles.PackageLoaders.Directory;
using FubuCore;
using FubuTestingSupport;

namespace Bottles.Tests.IntegrationTesting
{
    /*
       alias -> Manage folder aliases
  assemblies -> Adds assemblies to a given manifest
assembly-pak -> Bundle up the content and data files for a self contained assembly package
      create -> Create a bottle zip from a package directory
  create-pak -> Create a package file from a package directory
        help -> list all the available commands
        init -> Initialize a bottle manifest
        link -> Links a package folder to an application folder in development mode
     */



    public class IntegrationTestDriver
    {
        public static string SolutionDirectory = ".".ToFullPath().ParentDirectory().ParentDirectory().ParentDirectory().ParentDirectory();
        public static string StagingDirectory = SolutionDirectory.AppendPath("bottles-staging");
        public static string SourceDirectory = SolutionDirectory.AppendPath("src").AppendPath("BottleProject");
        public static string ZipsDirectory = SolutionDirectory.AppendPath("zips");

        private static string BottleRunnerFile =
            SolutionDirectory.AppendPath("src").AppendPath("Bottles.Console").AppendPath("bin").AppendPath("debug").
                AppendPath("BottleRunner.exe");

        public static void ResetBottleProjectCode()
        {
            var system = new FileSystem();



            system.CleanDirectory(StagingDirectory);
            Thread.Sleep(100);

            system.Copy(SourceDirectory, StagingDirectory);

            system.CreateDirectory(ZipsDirectory);
            system.CleanDirectory(ZipsDirectory);
        }

        public static void Recompile()
        {
            var info = new ProcessStartInfo
            {
                FileName = SolutionDirectory.AppendPath("compile-bottle-staging.cmd"),
                WorkingDirectory = SolutionDirectory

            };

            var returnCode = new ProcessRunner().Run(info, text => Debug.WriteLine(text));
            returnCode.ExitCode.ShouldEqual(0);
        }

        public static void SetAssemblyVersion(string version)
        {
            var file = StagingDirectory.AppendPath("version.cs");
            new FileSystem().AlterFlatFile(file, list => {
                list.Clear();

                list.Add("using System.Reflection;");
                list.Add(" ");
                list.Add("[assembly: AssemblyVersion(\"{0}\")]".ToFormat(version));
                list.Add("[assembly: AssemblyFileVersion(\"{0}\")]".ToFormat(version));
            });
        }

        public static void SetData(string value)
        {
            var file = StagingDirectory.AppendPath("data").AppendPath("1.txt");
            new FileSystem().WriteStringToFile(file, value);
        }

        public static void SetContent(string value)
        {
            var file = StagingDirectory.AppendPath("content").
                AppendPath("scripts").AppendPath("script1.js");
            new FileSystem().WriteStringToFile(file, value);
        }

        public static void RunBottlesCommand(string arguments)
        {
            var processInfo = new ProcessStartInfo(BottleRunnerFile)
            {
                Arguments = arguments,
                CreateNoWindow = true,
                WorkingDirectory = SolutionDirectory
            };

            var processReturn = new ProcessRunner().Run(processInfo, text => Debug.WriteLine(text));
            processReturn.ExitCode.ShouldEqual(0);
        }

        public static void AlterManifest(Action<PackageManifest> alteration)
        {
            var fileSystem = new FileSystem();
            var manifest = fileSystem.LoadPackageManifestFrom(StagingDirectory);

            alteration(manifest);

            fileSystem.WriteObjectToFile(StagingDirectory.AppendPath(PackageManifest.FILE), manifest);
        }
    }



    public class ProcessRunner
    {
        public ProcessReturn Run(ProcessStartInfo info, TimeSpan waitDuration, Action<string> callback)
        {
            //use the operating system shell to start the process
            //this allows credentials to flow through.
            //info.UseShellExecute = true; 
            info.UseShellExecute = false;
            info.Verb = "runas";
            info.WindowStyle = ProcessWindowStyle.Normal;

            //don't open a new terminal window
            info.CreateNoWindow = false;

            info.RedirectStandardError = info.RedirectStandardOutput = true;

            //if (!Path.IsPathRooted(info.FileName))
            //{
            //    info.FileName = info.WorkingDirectory.AppendPath(info.FileName);
            //}

            ProcessReturn returnValue = null;
            var output = new StringBuilder();
            int pid = 0;
            using (var proc = Process.Start(info))
            {
                pid = proc.Id;
                proc.OutputDataReceived += (sender, outputLine) =>
                {
                    if (outputLine.Data.IsNotEmpty())
                    {
                        callback(outputLine.Data);
                    }
                    output.AppendLine(outputLine.Data);
                };

                proc.BeginOutputReadLine();
                proc.WaitForExit((int)waitDuration.TotalMilliseconds);

                killProcessIfItStillExists(pid);

                returnValue = new ProcessReturn()
                {
                    ExitCode = proc.ExitCode,
                    OutputText = output.ToString()
                };
            }

            return returnValue;
        }

        private void killProcessIfItStillExists(int pid)
        {
            if (Process.GetProcesses()
                .Where(p => p.Id == pid)
                .Any())
            {
                try
                {
                    var p = Process.GetProcessById(pid);
                    if (!p.HasExited)
                    {
                        p.Kill();
                        Thread.Sleep(100);
                    }
                }
                catch (ArgumentException)
                {
                    //ignore
                }
            }
        }

        public ProcessReturn Run(ProcessStartInfo info, Action<string> callback)
        {
            return Run(info, new TimeSpan(0, 0, 0, 10), callback);
        }
    }

    public class ProcessReturn
    {
        public string OutputText { get; set; }
        public int ExitCode { get; set; }
    }

    public class BottleDomainProxy : MarshalByRefObject
    {
        private IPackageInfo bottle
        {
            get { return PackageRegistry.Packages.Single(x => x.Name == "BottlesProject"); }
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
                x.Loader(new ZipFilePackageLoader(IntegrationTestDriver.SolutionDirectory.AppendPath("exploded"), new string[]{folder}));
            });
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }

    public class BottleLoadingDomain : IDisposable
    {
        private Lazy<AppDomain> _domain;
        private Lazy<BottleDomainProxy> _proxy; 

        public BottleLoadingDomain()
        {
            Recycle();
        }

        public BottleDomainProxy Proxy
        {
            get { return _proxy.Value; }
        }

        public void Recycle()
        {
            Dispose();

            _domain = new Lazy<AppDomain>(() =>
            {
                var setup = new AppDomainSetup
                {
                    ApplicationName = "Bottles-Testing-" + Guid.NewGuid(),
                    ShadowCopyFiles = "true",
                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
                };

                return AppDomain.CreateDomain("Bottles-Testing", null, setup);
            });

            _proxy = new Lazy<BottleDomainProxy>(() =>
            {
                var proxyType = typeof(BottleDomainProxy);
                return (BottleDomainProxy)_domain.Value.CreateInstanceAndUnwrap(proxyType.Assembly.FullName, proxyType.FullName);
            });
        }

        public void Dispose()
        {
            if (_domain != null && _domain.IsValueCreated)
            {
                AppDomain.Unload(_domain.Value);
            }
        }
    }
}