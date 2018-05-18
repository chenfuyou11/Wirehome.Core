using Wirehome.Core.Interface.Native;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Raspberry.Core;

namespace Wirehome.Raspberry
{
    internal static class IContainerExtensions
    {
        public static void RegisterRaspberryServices(this IContainer container)
        {
            container.RegisterSingleton<INativeGpioController, RaspberryGpioController>();
            container.RegisterSingleton<INativeI2cBus, RaspberryI2cBus>();
            container.RegisterSingleton<INativeSerialDevice, RaspberrySerialDevice>();
            container.RegisterSingleton<INativeSoundPlayer, RaspberrySoundPlayer>();
            container.RegisterSingleton<INativeStorage, RaspberryStorage>();
            container.RegisterSingleton<INativeTimerSerice, RaspberryTimerSerice>();
        }
    }
}