using System;
using Windows.ApplicationModel.Background;
using Wirehome.Core;

namespace Wirehome.Controller.Main
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var hostname = global::System.Net.Dns.GetHostName();

            Type configurationType;
            if (hostname == "Wirehome-Main")
            {
                configurationType = typeof(Main.Configuration);
            }
            else if (hostname == "Wirehome-Cellar")
            {
                configurationType = typeof(Cellar.Configuration);
            }
            else
            {
                return;
            }

            var options = new ControllerOptions
            {
                ConfigurationType = configurationType
            };

            var controller = new Core.Controller(options);
            controller.RunAsync(taskInstance);
        }
    }
}