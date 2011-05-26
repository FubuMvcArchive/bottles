using System;
using System.Diagnostics;

namespace Bottles.Deployment
{
    public class ProcessRunner : IProcessRunner
    {
        public int Run(ProcessStartInfo info, TimeSpan waitDuration)
        {
            info.UseShellExecute = false; //don't start from cmd.exe
            info.CreateNoWindow = true; //don't use a window
            info.RedirectStandardOutput = true;

            int exitCode = 0;
            using (var proc = Process.Start(info))
            {
                using(var reader = new Process().StandardOutput)
                {
                    var result = reader.ReadToEnd();
                    Console.Write(result);
                }
                
                proc.WaitForExit((int)waitDuration.TotalMilliseconds);
                exitCode = proc.ExitCode;
            }
            return exitCode;
        }

        public int Run(ProcessStartInfo info)
        {
            return Run(info, new TimeSpan(0,0,0,10));
        }
    }
}