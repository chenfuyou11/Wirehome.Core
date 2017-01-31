using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.I2CHardwareBridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Extensions
{
    public static class DeviceServiceExtensions
    {
        public static DHT22TemperatureSensor GetTempSensor(this IDeviceService service, byte sensorID)
        {
            var i2cHardwareBridge = service.GetDevice<I2CHardwareBridge>();

            return i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor(sensorID);
        }

        public static DHT22HumiditySensor GetHumiditySensor(this IDeviceService service, byte sensorID)
        {
            var i2cHardwareBridge = service.GetDevice<I2CHardwareBridge>();

            return i2cHardwareBridge.DHT22Accessor.GetHumiditySensor(sensorID);
        }
    }
}
