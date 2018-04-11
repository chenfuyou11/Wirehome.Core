using System;

namespace Wirehome.Contracts.Core
{
    public class StartupFailedEventArgs : EventArgs
    {
        public StartupFailedEventArgs(TimeSpan duration, Exception exception)
        {
            Duration = duration;
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        public TimeSpan Duration { get; }
        public Exception Exception { get; }
    }
}
