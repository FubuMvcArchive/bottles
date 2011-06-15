using System;
using System.Diagnostics;
using System.Linq;

namespace Bottles.Deployment
{
    public class ProcessRunner : IProcessRunner
    {
        public int Run(ProcessStartInfo info, TimeSpan waitDuration)
        {
            //use the operating system shell to start the process
            //this allows credentials to flow through.
            info.UseShellExecute = true; 

            //don't open a new terminal window
            info.CreateNoWindow = true;

            int exitCode = 0;
            int pid = 0;
            using (var proc = Process.Start(info))
            {
                pid = proc.Id;
                proc.WaitForExit((int)waitDuration.TotalMilliseconds);
                exitCode = proc.ExitCode;
            }

            killProcessIfItStillExists(pid);

            return exitCode;
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

        public int Run(ProcessStartInfo info)
        {
            return Run(info, new TimeSpan(0,0,0,10));
        }
    }
}