using HA4IoT.Core;
using Windows.ApplicationModel.Background;
using HA4IoT.Extensions;

namespace HA4IoT.Controller.Dnf
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var options = new ControllerOptions
            {
                ConfigurationType = typeof(Configuration),
                ContainerConfigurator = new ContainerConfigurator()
            };

            options.LogAdapters.Add(new EtwLoggingService());

            var controller = new Core.Controller(options);
            controller.RunAsync(taskInstance);
        }
    }
}
