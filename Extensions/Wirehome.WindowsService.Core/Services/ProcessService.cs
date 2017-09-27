using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Wirehome.WindowsService.Services
{
    public static class ProcessService
    {
        public static void StartProcess(string path)
        {
            Process.Start(path);
        }

        public static void StopProcess(string name)
        {
            var active = GetActiveProcess(name);

            if(active.Any())
            {
                Process.GetProcessById(active.FirstOrDefault().PID);
            }
        }

        public static bool IsProcessStarted(string processName)
        {
            return GetActiveProcess(processName).Any();
        }
     
        private static IEnumerable<ProcessDetails> GetActiveProcess(string processIndicator)
        {
            var activeProcess = Process.GetProcesses().Select(x => new ProcessDetails { ProcessName = x.ProcessName, PID = x.Id }).ToList();

            var list = new List<ProcessDetails>();
            foreach (var item in activeProcess)
            {
                if (item.ProcessName.IndexOf(processIndicator, 0, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    list.Add(item);
                }
            }
            return list;
        }
    }
}
