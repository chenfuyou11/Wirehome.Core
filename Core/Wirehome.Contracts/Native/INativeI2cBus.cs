namespace Wirehome.Contracts.Core
{
    public interface INativeI2cBus
    {
        INativeI2cDevice CreateDevice(string deviceId, int slaveAddress);
        string GetBusId();
    }

}