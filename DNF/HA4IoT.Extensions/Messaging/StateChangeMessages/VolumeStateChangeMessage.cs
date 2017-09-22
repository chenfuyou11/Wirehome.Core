using HA4IoT.Extensions.Devices.States;

namespace HA4IoT.Extensions.Messaging.StateChangeMessages
{
    public class VolumeStateChangeMessage : StateChangeMessage<VolumeState>
    {
        public VolumeStateChangeMessage(string deviceID, VolumeState oldValue, VolumeState newValue) : base(deviceID, oldValue, newValue)
        {
        }
    }
}
