using HA4IoT.Contracts.Components.States;

namespace HA4IoT.Extensions.Messaging.StateChangeMessages
{
    public class PowerStateChangeMessage : StateChangeMessage<PowerState>
    {
        public PowerStateChangeMessage(string deviceID, PowerState oldValue, PowerState newValue) : base(deviceID, oldValue, newValue)
        {
        }
    }
}
