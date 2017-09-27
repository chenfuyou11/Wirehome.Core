using Wirehome.Contracts.Core;
using Wirehome.Raspberry.Core;

namespace Wirehome.Raspberry
{
    public static class IContainerExtensions
    {
        public static void RegisterRaspberryServices(IContainer container)
        {
            container.RegisterSingleton<IBinaryReader, BinaryReader>();
            container.RegisterSingleton<INativeBackgroundTask, RaspberryBackgroundTask>();
            container.RegisterSingleton<INativeGpio, RaspberryGpio>();
            container.RegisterSingleton<INativeGpioController, RaspberryGpioController>();
            container.RegisterSingleton<INativeI2cDevice, RaspberryI2cDevice>();
            container.RegisterSingleton<INativeSerialDevice, RaspberrySerialDevice>();
            container.RegisterSingleton<INativeSoundPlayer, RaspberrySoundPlayer>();
            container.RegisterSingleton<INativeStorage, RaspberryStorage>();
            container.RegisterSingleton<INativeTimerSerice, RaspberryTimerSerice>();
        }
    }
}

