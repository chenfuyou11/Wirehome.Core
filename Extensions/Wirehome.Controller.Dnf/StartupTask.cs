using Windows.ApplicationModel.Background;
using Wirehome.Core;

namespace Wirehome.Controller.Dnf
{
    public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            var options = new ControllerOptions
            {
                ConfigurationType = typeof(Configuration),
                ContainerConfigurator = new ContainerConfigurator()
            };

            var controller = new WirehomeController(options);
            if(!await controller.RunAsync())
            {
                deferral.Complete();
            }
        }
    }
}
