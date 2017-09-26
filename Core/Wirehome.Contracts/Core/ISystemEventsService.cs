using System;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Core
{
    public interface ISystemEventsService : IService
    {
        event EventHandler StartupCompleted;
        event EventHandler StartupFailed;
    }
}
