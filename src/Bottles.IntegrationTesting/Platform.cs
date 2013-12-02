using System;

namespace Bottles.IntegrationTesting
{
    public static class Platform
    {
        public static bool IsUnix ()
        {
            var pf = System.Environment.OSVersion.Platform;
            return pf == PlatformID.Unix || pf == PlatformID.MacOSX;
        }
    }
}

