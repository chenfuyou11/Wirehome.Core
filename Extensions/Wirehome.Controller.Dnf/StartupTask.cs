using Windows.ApplicationModel.Background;
using Wirehome.Core;
using Wirehome.Raspberry.Core;

namespace Wirehome.Controller.Dnf
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

            var controller = new Wirehome.Core.Controller(options);
            controller.RunAsync(new RaspberryBackgroundTask(taskInstance));
            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //
        }
    }
}
