using Wirehome.Extensions.Devices.States;

namespace Wirehome.Extensions.Messaging.StateChangeMessages
{
    public class PlaybackStateChangeMessage : StateChangeMessage<PlaybackState>
    {
        public PlaybackStateChangeMessage(string deviceID, PlaybackState oldValue, PlaybackState newValue) : base(deviceID, oldValue, newValue)
        {
        }
    }
}
