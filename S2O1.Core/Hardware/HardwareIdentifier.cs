using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;

namespace S2O1.Core.Hardware
{
    public static class HardwareIdentifier
    {
        public static string GetHardwareId()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsId();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Docker check?
                if (File.Exists("/.dockerenv"))
                {
                    return GetDockerId();
                }
                return GetLinuxId();
            }
            return "UNKNOWN-PLATFORM";
        }

        private static string GetWindowsId()
        {
            // Simplified: In real app use WMI (ManagementObjectSearcher)
            // But Core shouldn't depend on System.Management for cross-platform unless Nuget.
            // Using CLI command 'wmic' is a fallback.
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "wmic",
                        Arguments = "csproduct get UUID",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output.Split('\n', StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Trim() ?? "WIN-ERR";
            }
            catch
            {
                return "WIN-Exec-Err";
            }
        }

        private static string GetLinuxId()
        {
            // /sys/class/dmi/id/product_uuid
            try
            {
                if (File.Exists("/sys/class/dmi/id/product_uuid"))
                    return File.ReadAllText("/sys/class/dmi/id/product_uuid").Trim();
                
                 if (File.Exists("/etc/machine-id"))
                    return File.ReadAllText("/etc/machine-id").Trim();
            }
            catch {}
            return "LINUX-ERR";
        }

        private static string GetDockerId()
        {
            // Use hostname or mapped file
            try
            {
                // Hostname in docker is container ID usually
                return System.Net.Dns.GetHostName(); 
            }
            catch
            {
                return "DOCKER-ERR";
            }
        }
    }
}
