using Windows.ApplicationModel.Background;
using Wirehome.Core;

namespace Wirehome.Controller.Dnf
{
    public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            
            //var controller = new WirehomeController(options);
            //if(!await controller.RunAsync().ConfigureAwait(false))
            //{
            //    deferral.Complete();
            //}

            deferral.Complete();
        }
    }
}
