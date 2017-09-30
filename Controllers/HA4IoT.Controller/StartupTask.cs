using Windows.ApplicationModel.Background;
using Wirehome.Core;

namespace Wirehome.Controller
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var options = new ControllerOptions();
            var controller = new Core.Controller(options);
            controller.RunAsync(taskInstance);
        }
    }
}
