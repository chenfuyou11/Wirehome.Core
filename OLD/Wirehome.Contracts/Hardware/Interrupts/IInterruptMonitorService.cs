using System;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Hardware.Interrupts
{
    public interface IInterruptMonitorService : IService
    {
        void RegisterCallback(string interruptMonitorId, Action callback);
    }
}