using Wirehome.Extensions.Devices.States;

namespace Wirehome.Extensions.Messaging.StateChangeMessages
{
    public class InpuSourceChangeMessage : StateChangeMessage<InputSourceState>
    {
        public InpuSourceChangeMessage(string deviceID, InputSourceState oldValue, InputSourceState newValue) : base(deviceID, oldValue, newValue)
        {
        }
    }
}
