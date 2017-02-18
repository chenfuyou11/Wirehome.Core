using HA4IoT.Core;
using Windows.ApplicationModel.Background;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts;
using HA4IoT.Extensions;
using HA4IoT.Contracts.Core;
using System;

namespace HA4IoT.Controller.Dnf
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const byte RASPBERRY_LED = 22;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Log.SeverityLevel = LogSeverityLevel.Error;

            var options = new ControllerOptions
            {
                StatusLedNumber = RASPBERRY_LED,
                ConfigurationType = typeof(Configuration),
                ContainerConfigurator = new ContainerConfigurator()
            };

            var controller = new Core.Controller(options);
            controller.RunAsync(taskInstance);
        }
    }
}
