namespace Wirehome.Core.Native
{
    public interface INativeI2cBus
    {
        INativeI2cDevice CreateDevice(string deviceId, int slaveAddress);
        string GetBusId();
    }

}