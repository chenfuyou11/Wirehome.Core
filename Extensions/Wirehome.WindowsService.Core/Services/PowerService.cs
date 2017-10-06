using Wirehome.WindowsService.Interop;
using System.Diagnostics;

namespace Wirehome.WindowsService.Services
{
    public static class PowerService
    {
        public static void SetPowerMode(PowerState powerState)
        {
            switch (powerState)
            {
                case PowerState.Hibernate:
                    Win32Api.SetSuspendState(true, true, true);
                    break;
                case PowerState.Sleep:
                    Win32Api.SetSuspendState(false, true, true);
                    break;
                case PowerState.Shutdown:
                    Process.Start("shutdown", "/s /t 0");
                    break;
                case PowerState.Restart:
                    Process.Start("shutdown", "/r /t 0");
                    break;
                case PowerState.LogOff:
                    Win32Api.ExitWindowsEx(0, 0);
                    break;
                case PowerState.Lock:
                    Win32Api.LockWorkStation();
                    break;
            }
        }
    }
}
