using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using Wirehome.Core.Interface.Native;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Core.Services.Logging;
using Wirehome.Model.Core;
using Wirehome.Raspberry;

namespace Wirehome.Controller
{
    public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
                       
            try
            { 
                await new WirehomeController(GetControllerOptions()).Initialize().ConfigureAwait(false);
            }
            catch (System.Exception)
            {
                deferral.Complete();
            }
        }

        private ControllerOptions GetControllerOptions() => new ControllerOptions
        {
            NativeServicesRegistration = RegisterRaspberryServices,
            AdapterMode = AdapterMode.Embedded,
            Loggers = new List<ILogAdapter> { new RaspberryLoggingService() }
        };

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
