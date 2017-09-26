using System;

namespace Wirehome.Contracts.Core
{
    public sealed class TimerTickEventArgs : EventArgs
    {
        public TimeSpan ElapsedTime { get; set; }
    }
}
