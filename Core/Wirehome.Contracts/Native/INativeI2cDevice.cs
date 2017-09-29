namespace Wirehome.Contracts.Core
{
    public interface INativeI2cDevice
    {
        void Dispose();
        NativeI2cTransferResult WritePartial(byte[] buffer);
        NativeI2cTransferResult ReadPartial(byte[] buffer);
        NativeI2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer);
    }

}