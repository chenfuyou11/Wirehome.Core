using Wirehome.Contracts.Components.States;

namespace Wirehome.Extensions.Messaging.StateChangeMessages
{
    public class PowerStateChangeMessage : StateChangeMessage<PowerState>
    {
        public PowerStateChangeMessage(string deviceID, PowerState oldValue, PowerState newValue) : base(deviceID, oldValue, newValue)
        {
        }
    }
}
