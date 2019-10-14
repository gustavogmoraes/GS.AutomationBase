using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AutomationBase
{
    public static class DevOpsHelper
    {
        // Infra Helper
        public static OSPlatform GetOsPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSPlatform.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux;
            }

            return OSPlatform.Create("Other");
        }
    }
}
