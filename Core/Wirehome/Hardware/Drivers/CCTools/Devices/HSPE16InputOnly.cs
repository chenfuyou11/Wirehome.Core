using Wirehome.Contracts.Core;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Hardware.I2C;
using Wirehome.Contracts.Logging;
using Wirehome.Hardware.Drivers.I2CPortExpanderDrivers;

namespace Wirehome.Hardware.Drivers.CCTools.Devices
{
    public sealed class HSPE16InputOnly : CCToolsDeviceBase
    {
        public HSPE16InputOnly(string id, I2CSlaveAddress address, II2CBusService i2CBusService, ILogger log)
            : base(id, new MAX7311Driver(address, i2CBusService), log)
        {
            byte[] setupAsInputs = { 0x06, 0xFF, 0xFF };
            i2CBusService.Write(address, setupAsInputs);

            FetchState();
        }

        public IBinaryInput GetInput(int number)
        {
            // All ports have a pullup resistor.
            return ((IBinaryInput)GetPort(number)).WithInvertedState();
        }

        public IBinaryInput this[HSPE16Pin pin] => GetInput((int)pin);
    }
}
