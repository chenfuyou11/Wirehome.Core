using System;
using Wirehome.Contracts.Devices;
using Wirehome.Contracts.Devices.Configuration;
using Wirehome.Contracts.Hardware.I2C.I2CHardwareBridge.Configuration;
using Wirehome.Contracts.Hardware.Outpost.Configuration;
using Wirehome.Hardware.Drivers.I2CHardwareBridge.Configuration;

namespace Wirehome.Hardware.Drivers.Outpost
{
    public class OutpostDeviceFactory : IDeviceFactory
    {
        private readonly OutpostDeviceService _outpostDeviceService;

        public OutpostDeviceFactory(OutpostDeviceService outpostDeviceService)
        {
            _outpostDeviceService = outpostDeviceService ?? throw new ArgumentNullException(nameof(outpostDeviceService));
        }

        public bool TryCreateDevice(string id, DeviceConfiguration deviceConfiguration, out IDevice device)
        {
            switch (deviceConfiguration.Driver.Type)
            {
                case "Outpost.LpdBridge":
                {
                    return CreateGetLpdBridgeAdapter(id, deviceConfiguration, out device);
                }

                case "I2CHardwareBridge":
                {
                    return CreateI2CHardwareBridge(id, deviceConfiguration, out device);
                }

                case "I2CLdp433MhzBridge":
                {
                     return CreateI2CLdp433MhzBridge(id, deviceConfiguration, out device);
                }
            }

            device = null;
            return false;
        }

        private bool CreateGetLpdBridgeAdapter(string id, DeviceConfiguration deviceConfiguration, out IDevice device)
        {
            var configuration = deviceConfiguration.Driver.Parameters.ToObject<LpdBridgeConfiguration>();

            device = _outpostDeviceService.CreateLpdBridgeAdapter(id, configuration.DeviceName);
            return true;
        }

        private bool CreateI2CHardwareBridge(string id, DeviceConfiguration deviceConfiguration, out IDevice device)
        {
            var configuration = deviceConfiguration.Driver.Parameters.ToObject<I2CHardwareBridgeConfiguration>();
            device = _outpostDeviceService.CreateI2CHardwareBridge(id, configuration.Address);
            return true;
        }

        private bool CreateI2CLdp433MhzBridge(string id, DeviceConfiguration deviceConfiguration, out IDevice device)
        {
            var configuration = deviceConfiguration.Driver.Parameters.ToObject<I2CLdp433MhzBridgeConfiguration>();
            device = _outpostDeviceService.CreateLdp433MhzBridgeAdapter(id, configuration.Pin);
            return true;
        }
    }
}
