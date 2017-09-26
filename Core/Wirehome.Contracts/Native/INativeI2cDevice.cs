namespace Wirehome.Contracts.Core
{
    public interface INativeI2cDevice
    {
        INativeI2cDevice CreateDevice(string deviceId, int slaveAddress);
        string GetBusId();
        void Dispose();
        NativeI2cTransferResult WritePartial(byte[] buffer);
        NativeI2cTransferResult ReadPartial(byte[] buffer);
        NativeI2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer);
    }

}