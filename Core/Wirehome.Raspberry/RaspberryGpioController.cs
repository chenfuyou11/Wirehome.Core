using Windows.Devices.Gpio;
using Wirehome.Contracts.Core;

namespace Wirehome.Raspberry
{
    public class RaspberryGpioController : INativeGpioController
    {
        private readonly GpioController _gpioController;

        public RaspberryGpioController() => _gpioController = GpioController.GetDefault();

        public INativeGpio OpenPin(int pinNumber, NativeGpioSharingMode sharingMode) => new RaspberryGpio(_gpioController.OpenPin(pinNumber, (GpioSharingMode)sharingMode));
    }
}
