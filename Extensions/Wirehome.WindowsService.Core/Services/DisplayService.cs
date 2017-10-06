using System;
using System.Runtime.InteropServices;

namespace Wirehome.WindowsService.Core
{
    //https://stackoverflow.com/questions/6590939/how-to-set-display-settings-to-extend-mode-in-windows-7-using-c
    //https://stackoverflow.com/questions/16790287/programmatically-changing-the-presentation-display-mode

    public static class DisplayService
    {
        public static void SetDisplayMode(DisplayMode mode)
        {
            var proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "DisplaySwitch.exe";
            switch (mode)
            {
                case DisplayMode.External:
                    proc.StartInfo.Arguments = "/external";
                    break;
                case DisplayMode.Internal:
                    proc.StartInfo.Arguments = "/internal";
                    break;
                case DisplayMode.Extend:
                    proc.StartInfo.Arguments = "/extend";
                    break;
                case DisplayMode.Duplicate:
                    proc.StartInfo.Arguments = "/clone";
                    break;
            }
            proc.Start();
        }
       
    }
}
