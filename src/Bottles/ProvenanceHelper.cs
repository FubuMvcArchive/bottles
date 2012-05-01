using System;
using System.Diagnostics;
using System.Threading;
using FubuCore;
using FubuCore.Reflection;

namespace Bottles
{
    public static class ProvenanceHelper
    {
        public static string GetProvenanceFromStack()
        {
            var theBottleAssembly = typeof(IPackageInfo).Assembly; //bottle assembly
            var trace = new StackTrace(Thread.CurrentThread, false);

            //walk the stack looking for the first 'valid' frame to use
            for (var i = 0; i < trace.FrameCount; i++)
            {
                var frame = trace.GetFrame(i);
                var assembly = frame.GetMethod().DeclaringType.Assembly;

                if (assembly == theBottleAssembly) continue;
                if (!frame.GetMethod().HasAttribute<SkipOverForProvenanceAttribute>())
                {
                    return frame.ToDescription();
                }
            }

            return "Unknown";
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SkipOverForProvenanceAttribute : Attribute
    {

    }
}