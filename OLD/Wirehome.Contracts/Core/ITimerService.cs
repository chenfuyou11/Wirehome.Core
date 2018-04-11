using System;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Core
{
    public interface ITimerService : IService
    {
        event EventHandler<TimerTickEventArgs> Tick;
    }
}
