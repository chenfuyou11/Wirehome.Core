using System;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Hardware.I2C;
using Wirehome.Contracts.Logging;
using Wirehome.Hardware.Drivers.I2CPortExpanderDrivers;

namespace Wirehome.Hardware.Drivers.CCTools.Devices
{
    public class HSRT16 : CCToolsDeviceBase
    {
        public HSRT16(string id, I2CSlaveAddress address, II2CBusService i2CBusService, ILogger log)
            : base(id, new MAX7311Driver(address, i2CBusService), log)
        {
            SetState(new byte[] { 0x00, 0x00 });
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            if (number < 0 || number > 15) throw new ArgumentOutOfRangeException(nameof(number));

            return GetPort(number);
        }

        public IBinaryOutput this[HSRT16Pin pin] => GetOutput((int) pin);
    }
}