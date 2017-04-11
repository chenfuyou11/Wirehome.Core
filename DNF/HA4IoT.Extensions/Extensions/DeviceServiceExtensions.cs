using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.I2C.I2CHardwareBridge;

namespace HA4IoT.Extensions.Extensions
{
    public static class DeviceServiceExtensions
    {
        public static DHT22TemperatureSensor GetTempSensor(this IDeviceRegistryService service, byte sensorID)
        {
            var i2cHardwareBridge = service.GetDevice<I2CHardwareBridge>();

            return i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(sensorID);
        }

        public static DHT22HumiditySensor GetHumiditySensor(this IDeviceRegistryService service, byte sensorID)
        {
            var i2cHardwareBridge = service.GetDevice<I2CHardwareBridge>();

            return i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(sensorID);
        }
    }
}
