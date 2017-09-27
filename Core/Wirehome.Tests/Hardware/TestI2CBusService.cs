using System;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Hardware.I2C;
using Wirehome.Contracts.Services;
using Wirehome.Hardware.I2C;

namespace Wirehome.Tests.Mockups.Services
{
    public class TestI2CBusService : ServiceBase, II2CBusService
    {
        public I2CSlaveAddress LastUsedI2CSlaveAddress { get; private set; }

        public byte[] LastWrittenBytes { get; private set; }

        public byte[] BufferForNextRead { get; set; }

        public II2CTransferResult Write(I2CSlaveAddress address, byte[] buffer, bool useCache = true)
        {
            LastUsedI2CSlaveAddress = address;
            LastWrittenBytes = buffer;
            return new I2CTransferResult(I2CTransferStatus.FullTransfer, buffer.Length);
        }

        public II2CTransferResult Read(I2CSlaveAddress address, byte[] buffer, bool useCache = true)
        {
            LastUsedI2CSlaveAddress = address;
            Array.Copy(BufferForNextRead, buffer, buffer.Length);
            return new I2CTransferResult(I2CTransferStatus.FullTransfer, buffer.Length);
        }

        public II2CTransferResult WriteRead(I2CSlaveAddress address, byte[] writeBuffer, byte[] readBuffer, bool useCache = true)
        {
            LastUsedI2CSlaveAddress = address;
            LastWrittenBytes = writeBuffer;
            Array.Copy(BufferForNextRead, readBuffer, readBuffer.Length);
            return new I2CTransferResult(I2CTransferStatus.FullTransfer, writeBuffer.Length + readBuffer.Length);
        }
    }
}
