using System;

namespace Wirehome.Contracts.Core
{
    public class StartupCompletedEventArgs : EventArgs
    {
        public StartupCompletedEventArgs(TimeSpan duration)
        {
            Duration = duration;
        }

        public TimeSpan Duration { get; }
    }
}
