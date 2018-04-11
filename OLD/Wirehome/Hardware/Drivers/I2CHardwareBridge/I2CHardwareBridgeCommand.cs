using Wirehome.Contracts.Core;
using Wirehome.Contracts.Hardware.I2C;

namespace Wirehome.Hardware.Drivers.I2CHardwareBridge
{
    public abstract class I2CHardwareBridgeCommand
    {
        public abstract void Execute(I2CSlaveAddress address, II2CBusService i2CBusService);
    }
}
