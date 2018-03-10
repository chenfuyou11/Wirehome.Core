namespace Wirehome.Core.Native
{
    public interface INativeGpioController
    {
        INativeGpio OpenPin(int pinNumber, NativeGpioSharingMode sharingMode);
    }
}