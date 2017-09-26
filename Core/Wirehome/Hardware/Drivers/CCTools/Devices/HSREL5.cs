using System;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Hardware.I2C;
using Wirehome.Contracts.Logging;
using Wirehome.Hardware.Drivers.I2CPortExpanderDrivers;

namespace Wirehome.Hardware.Drivers.CCTools.Devices
{
    public sealed class HSREL5 : CCToolsDeviceBase
    {
        public HSREL5(string id, I2CSlaveAddress i2CAddress, II2CBusService i2CBusService, ILogger log)
            : base(id, new PCF8574Driver(i2CAddress, i2CBusService), log)
        {
            // Ensure that all relays are off by default. The first 5 ports are hardware inverted! The other ports are not inverted but the
            // connected relays are inverted.
            SetState(new byte[] { 0xFF });
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            if (number < 0 || number > 7) throw new ArgumentOutOfRangeException(nameof(number));

            var port = GetPort(number);
            if (number <= 4)
            {
                // The 4 relays have an hardware inverter. Invert here again to ensure that high means "on".
                return port.WithInvertedState();
            }

            return port;
        }

        public IBinaryOutput this[HSREL5Pin pin] => GetOutput((int) pin);
    }
}