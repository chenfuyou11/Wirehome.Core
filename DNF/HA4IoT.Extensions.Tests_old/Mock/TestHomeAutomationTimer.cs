using System;
using HA4IoT.Contracts.Core;


namespace HA4IoT.Extensions.Tests
{
    public class TestTimerService : ITimerService
    {
        public event EventHandler<TimerTickEventArgs> Tick;

        public void ExecuteTick(TimeSpan elapsedTime)
        {
            Tick?.Invoke(this, new TimerTickEventArgs { ElapsedTime = elapsedTime });
        }

        public void Startup()
        {
        }
    }
}
