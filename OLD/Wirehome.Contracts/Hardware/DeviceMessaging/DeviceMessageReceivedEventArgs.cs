using System;

namespace Wirehome.Contracts.Hardware.DeviceMessaging
{
    public class DeviceMessageReceivedEventArgs
    {
        public DeviceMessageReceivedEventArgs(DeviceMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public DeviceMessage Message { get; }
    }
}
