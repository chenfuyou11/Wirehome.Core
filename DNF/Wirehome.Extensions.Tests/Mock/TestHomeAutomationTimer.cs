using System;
using Wirehome.Contracts.Core;


namespace Wirehome.Extensions.Tests
{
    public class TestTimerService : ITimerService
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

        public void Startup()
        {
        }
    }
}
