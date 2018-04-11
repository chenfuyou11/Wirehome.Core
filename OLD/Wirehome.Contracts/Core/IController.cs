using System;

namespace Wirehome.Contracts.Core
{
    public interface IController
    {
        event EventHandler<StartupCompletedEventArgs> StartupCompleted;
        event EventHandler<StartupFailedEventArgs> StartupFailed;
    }
}
