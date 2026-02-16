using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace S2O1.CLI.Helpers
{
    public static class InstallationHelper
    {
        private const string LinuxFlagPath = "/etc/2s1o/installed.flag";
        
        public static bool IsInstalled()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // HKEY_LOCAL_MACHINE\Software\2S1O\Installed = true
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return false; // Safety
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(@"Software\2S1O");
                    if (key != null)
                    {
                        var installed = key.GetValue("Installed");
                        return installed != null && installed.ToString().ToLower() == "true";
                    }
                }
                catch (Exception) { /* Handle permission issue? */ }
                return false;
            }
            else
            {
                return File.Exists(LinuxFlagPath);
            }
        }

        public static void MarkAsInstalled()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                 try
                {
                    using var key = Registry.LocalMachine.CreateSubKey(@"Software\2S1O");
                    key.SetValue("Installed", "true");
                    key.SetValue("InstallDate", DateTime.Now.ToString("yyyy-MM-dd"));
                }
                catch (UnauthorizedAccessException)
                {
                    throw new Exception("Administrator privileges required to write to Registry.");
                }
            }
            else
            {
                try
                {
                    var dir = Path.GetDirectoryName(LinuxFlagPath);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(LinuxFlagPath, $"Installed={DateTime.Now:O}");
                }
                catch (UnauthorizedAccessException)
                {
                    throw new Exception("Root privileges required to write to /etc/.");
                }
            }
        }
    }
}
