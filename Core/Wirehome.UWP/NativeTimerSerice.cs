using System;
using Windows.System.Threading;
using Wirehome.Contracts.Core;

namespace Wirehome.UWP.Core
{
    public class NativeTimerSerice : INativeTimerSerice
    {
        public void CreatePeriodicTimer(Action action, TimeSpan interval)
        {
            ThreadPoolTimer.CreatePeriodicTimer(x => action(), interval);
        }
    }
}
