using System.Collections.Generic;
using Wirehome.Contracts.Devices;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Core
{
    public interface IDeviceRegistryService : IService
    {
        void RegisterDevice(IDevice device);

        TDevice GetDevice<TDevice>(string id) where TDevice : IDevice;

        TDevice GetDevice<TDevice>() where TDevice : IDevice;

        IList<TDevice> GetDevices<TDevice>() where TDevice : IDevice;

        void RegisterDeviceFactory(IDeviceFactory deviceFactory);
    }
}
