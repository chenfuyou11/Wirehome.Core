using System;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Hardware.I2C;
using Wirehome.Contracts.Logging;
using Wirehome.Hardware.Drivers.I2CPortExpanderDrivers;

namespace Wirehome.Hardware.Drivers.CCTools.Devices
{
    public sealed class HSPE8InputOnly : CCToolsDeviceBase
    {
        public HSPE8InputOnly(string id, I2CSlaveAddress address, II2CBusService i2CBusService, ILogger log)
            : base(id, new PCF8574Driver(address, i2CBusService), log)
        {
            FetchState();
        }

        public IBinaryInput GetInput(int number)
        {
            if (number < 0 || number > 7) throw new ArgumentOutOfRangeException(nameof(number));

            return GetPort(number);
        }

        public IBinaryInput this[HSPE8Pin pin] => GetInput((int)pin);
    }
}
