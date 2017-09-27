using Wirehome.WindowsService.Interop;
using System.Diagnostics;

namespace Wirehome.WindowsService.Services
{
    public static class PowerService
    {
        public static void Hibernate()
        {
            Win32Api.SetSuspendState(true, true, true);
        }

        public static void Sleep()
        {
            Win32Api.SetSuspendState(false, true, true);
        }

        public static void Shutdown()
        {
            Process.Start("shutdown", "/s /t 0"); 
        }

        public static void Restart()
        {
            Process.Start("shutdown", "/r /t 0");
        }

        public static void LogOff()
        {
            Win32Api.ExitWindowsEx(0, 0);
        }

        public static void Lock()
        {
            Win32Api.LockWorkStation();
        }
    }
}
