using Wirehome.Extensions.Devices.States;

namespace Wirehome.Extensions.Messaging.StateChangeMessages
{
    public class SurroundChangeMessage : StateChangeMessage<SurroundState>
    {
        public SurroundChangeMessage(string deviceID, SurroundState oldValue, SurroundState newValue) : base(deviceID, oldValue, newValue)
        {
        }
    }
}
