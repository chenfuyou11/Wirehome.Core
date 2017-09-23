using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Tests.Mockups
{
    public class TestTimerService : ServiceBase, ITimerService
    {
        public event EventHandler<TimerTickEventArgs> Tick;

        public void CreatePeriodicTimer(Action action, TimeSpan period)
        {
            throw new NotImplementedException();
        }

        public void ExecuteTick(TimeSpan elapsedTime)
        {
            Tick?.Invoke(this, new TimerTickEventArgs { ElapsedTime = elapsedTime });
        }
    }
}
