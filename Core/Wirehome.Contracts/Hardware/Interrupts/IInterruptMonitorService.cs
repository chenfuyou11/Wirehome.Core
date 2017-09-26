using System;

namespace Wirehome.Contracts.Hardware.Interrupts
{
    public interface IInterruptMonitorService
    {
        void RegisterInterrupts();
        void RegisterCallback(string interruptMonitorId, Action callback);
    }
}