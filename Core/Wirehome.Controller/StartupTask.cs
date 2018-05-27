using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Wirehome.ComponentModel;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.ComponentModel.Areas;
using Wirehome.Core.Hardware.RemoteSockets;
using Wirehome.Core.Interface.Native;
using Wirehome.Core.Services.DependencyInjection;
//using Wirehome.Core.Utils;
//using Wirehome.Model.Core;
using Wirehome.Raspberry;

namespace Wirehome.Controller
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            Task.Run(() =>
            {
                try
                {
                    var code = new DipswitchCode(DipswitchSystemCode.AllOff, DipswitchUnitCode.A, RemoteSocketCommand.TurnOff);
                    var tst = code.ToShortCode();

                    var x = new Area();
                    x.Test(5);
                    //var options = GetControllerOptions();
                    //var controller = new WirehomeController(options);

                    //controller.Initialize().GetAwaiter().GetResult();
                }
                catch (System.Exception)
                {
                    deferral.Complete();
                }
            });
          
        }

        //private ControllerOptions GetControllerOptions()
        //{
        //    var options = new ControllerOptions { AdapterRepository = @".\Adapters" };
        //    options.NativeServicesRegistration = RegisterRaspberryServices;

        //    return options;
        //}

        //private void RegisterRaspberryServices(IContainer container)
        //{
        //    container.RegisterSingleton<INativeGpioController, RaspberryGpioController>();
        //    container.RegisterSingleton<INativeI2cBus, RaspberryI2cBus>();
        //    container.RegisterSingleton<INativeSerialDevice, RaspberrySerialDevice>();
        //    container.RegisterSingleton<INativeSoundPlayer, RaspberrySoundPlayer>();
        //    container.RegisterSingleton<INativeStorage, RaspberryStorage>();
        //    container.RegisterSingleton<INativeTimerSerice, RaspberryTimerSerice>();
        //}
    }
}
