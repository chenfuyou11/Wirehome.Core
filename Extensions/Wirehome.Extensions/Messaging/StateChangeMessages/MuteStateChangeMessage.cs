using Wirehome.Extensions.Devices.States;

namespace Wirehome.Extensions.Messaging.StateChangeMessages
{
    public class MuteStateChangeMessage : StateChangeMessage<MuteState>
    {
        public MuteStateChangeMessage(string deviceID, MuteState oldValue, MuteState newValue) : base(deviceID, oldValue, newValue)
        {
        }
    }
}
