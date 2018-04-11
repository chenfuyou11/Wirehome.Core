namespace Wirehome.Contracts.Core
{
    public interface INativeGpioController
    {
        INativeGpio OpenPin(int pinNumber, NativeGpioSharingMode sharingMode);
    }
}