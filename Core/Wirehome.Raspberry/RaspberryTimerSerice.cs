using System;
using Windows.System.Threading;
using Wirehome.Contracts.Core;

namespace Wirehome.Raspberry.Core
{
    public class RaspberryTimerSerice : INativeTimerSerice
    {
        public void CreatePeriodicTimer(Action action, TimeSpan interval)
        {
            ThreadPoolTimer.CreatePeriodicTimer(x => action(), interval);
        }
    }
}
