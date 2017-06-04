using HA4IoT.Core;
using Windows.ApplicationModel.Background;
using HA4IoT.Contracts.Logging;
using HA4IoT.Extensions;

namespace HA4IoT.Controller.Dnf
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const byte RASPBERRY_LED = 22;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            //  \\192.168.0.106\c$\Data\Users\DefaultAccount\AppData\Local\Packages

            //TODO
            //Log.SeverityLevel = LogSeverityLevel.Error;

            var options = new ControllerOptions
            {
                //StatusLedGpio = RASPBERRY_LED,
                ConfigurationType = typeof(Configuration),
                ContainerConfigurator = new ContainerConfigurator()
            };

            options.LogAdapters.Add(new EtwLoggingService());

            var controller = new Core.Controller(options);
            controller.RunAsync(taskInstance);
        }
    }
}
