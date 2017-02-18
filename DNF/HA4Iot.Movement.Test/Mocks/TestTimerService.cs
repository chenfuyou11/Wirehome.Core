using System;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Core;

namespace UnitTestProject1
{
    public class TestTimerService : ITimerService
    {
        public event EventHandler<TimerTickEventArgs> Tick;

        public void Run()
        {
        }

        public void ExecuteTick(TimeSpan elapsedTime)
        {
            Tick?.Invoke(this, new TimerTickEventArgs(elapsedTime));
        }

        public void Startup()
        {
        }
    }
}
