using Windows.Devices.Gpio;
using Wirehome.Contracts.Core;

namespace Wirehome.UWP
{
    public class NativeGpioController : INativeGpioController
    {
        private readonly GpioController _gpioController;

        public NativeGpioController()
        {
            _gpioController = GpioController.GetDefault();
        }

        public INativeGpio OpenPin(int pinNumber, NativeGpioSharingMode sharingMode)
        {
            return new NativeGpio(_gpioController.OpenPin(pinNumber, (GpioSharingMode)sharingMode));
        }
    }
}
