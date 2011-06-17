using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployment
{
    public class ProcessReturn
    {
        public string OutputText { get; set; }
        public int ExitCode { get; set; }

        public void AssertOptionalSuccess()
        {
            LogWriter.Trace(OutputText);
        }

        public void AssertMandatorySuccess()
        {
            AssertMandatorySuccess(code => code > 0);
        }

        public void AssertMandatorySuccess(Func<int, bool> exitCodeFails)
        {
            if (exitCodeFails(ExitCode))
            {
                LogWriter.Fail(OutputText);
            }
            else
            {
                LogWriter.Trace(OutputText);
            }
        }
    }


    public class ProcessRunner : IProcessRunner
    {
        public ProcessReturn Run(ProcessStartInfo info, TimeSpan waitDuration)
        {
            //use the operating system shell to start the process
            //this allows credentials to flow through.
            //info.UseShellExecute = true; 
            info.UseShellExecute = false;
            info.Verb = "runas";

            //don't open a new terminal window
            info.CreateNoWindow = true;

            info.RedirectStandardError = info.RedirectStandardOutput = true;

            LogWriter.Trace("Running process at {0} {1}\nIn working directory {2}", info.FileName, info.Arguments, info.WorkingDirectory);
            
            if (!Path.IsPathRooted(info.FileName))
            {
                info.FileName = info.WorkingDirectory.AppendPath(info.FileName);
            }

            ProcessReturn returnValue = null;
            int pid = 0;
            using (var proc = Process.Start(info))
            {
                pid = proc.Id;
                proc.WaitForExit((int)waitDuration.TotalMilliseconds);

                returnValue = new ProcessReturn(){
                    ExitCode = proc.ExitCode,
                    OutputText = proc.StandardOutput.ReadToEnd()
                };                
            }

            killProcessIfItStillExists(pid);

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
                    Process.GetProcessById(pid).Kill();
                }
                catch (ArgumentException)
                {
                    //ignore
                }
            }
        }

        public ProcessReturn Run(ProcessStartInfo info)
        {
            return Run(info, new TimeSpan(0,0,0,10));
        }
    }
}