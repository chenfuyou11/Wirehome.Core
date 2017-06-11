using HA4IoT.Contracts.Core;
using HA4IoT.Hardware.Drivers.I2CHardwareBridge;

namespace HA4IoT.Extensions.Extensions
{
    public static class DeviceServiceExtensions
    {
        public static Dht22Sensor GetTempSensor(this IDeviceRegistryService service, byte sensorID)
        {
            var i2cHardwareBridge = service.GetDevice<I2CHardwareBridge>();

            return i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(sensorID);
        }

        public static Dht22Sensor GetHumiditySensor(this IDeviceRegistryService service, byte sensorID)
        {
            var i2cHardwareBridge = service.GetDevice<I2CHardwareBridge>();

            return i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(sensorID);
        }
    }
}
