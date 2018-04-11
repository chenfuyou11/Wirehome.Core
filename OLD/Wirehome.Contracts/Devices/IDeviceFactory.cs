using Wirehome.Contracts.Devices.Configuration;

namespace Wirehome.Contracts.Devices
{
    public interface IDeviceFactory
    {
        bool TryCreateDevice(string id, DeviceConfiguration deviceConfiguration, out IDevice device);
    }
}
