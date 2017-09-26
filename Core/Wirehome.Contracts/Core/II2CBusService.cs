using Wirehome.Contracts.Hardware.I2C;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Core
{
    public interface II2CBusService : IService
    {
        II2CTransferResult Write(I2CSlaveAddress address, byte[] buffer, bool useCache = true);

        II2CTransferResult Read(I2CSlaveAddress address, byte[] buffer, bool useCache = true);

        II2CTransferResult WriteRead(I2CSlaveAddress address, byte[] writeBuffer, byte[] readBuffer, bool useCache = true);
    }
}
