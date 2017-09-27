using Wirehome.Extensions.Devices.States;

namespace Wirehome.Extensions.Messaging.StateChangeMessages
{
    public class VolumeStateChangeMessage : StateChangeMessage<VolumeState>
    {
        public VolumeStateChangeMessage(string deviceID, VolumeState oldValue, VolumeState newValue) : base(deviceID, oldValue, newValue)
        {
        }
    }
}
