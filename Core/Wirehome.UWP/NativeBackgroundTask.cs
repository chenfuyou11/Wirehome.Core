using System;
using Windows.ApplicationModel.Background;
using Wirehome.Contracts.Core;

namespace Wirehome.UWP.Core
{
    public class NativeBackgroundTask : INativeBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        public NativeBackgroundTask(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance?.GetDeferral() ?? throw new ArgumentNullException(nameof(taskInstance));
        }

        public void Complete()
        {
            _deferral?.Complete();
        }
    }
}
