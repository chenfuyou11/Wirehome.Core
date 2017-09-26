using System;

namespace Wirehome.Contracts.Devices
{
    public class DeviceNotRegisteredException : Exception
    {
        public DeviceNotRegisteredException(string deviceId) : base("Device with ID '" + deviceId + "' is not registered.")
        {
        }
    }
}
