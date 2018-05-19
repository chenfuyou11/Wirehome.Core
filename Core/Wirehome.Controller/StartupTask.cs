using System.IO;
using Windows.ApplicationModel.Background;
using Wirehome.Core;
using Wirehome.Core.Interface.Native;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Model.Core;
using Wirehome.Raspberry;

namespace Wirehome.Controller.Dnf
{
    public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            if (!await new WirehomeController(GetControllerOptions()).Run().ConfigureAwait(false))
            {
                deferral.Complete();
            }
        }

        private ControllerOptions GetControllerOptions()
        {
            var options = new ControllerOptions { AdapterRepository = @".\Adapters" };
            options.NativeServicesRegistration = RegisterRaspberryServices;

            return options;
        }

        private void RegisterRaspberryServices(IContainer container)
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
