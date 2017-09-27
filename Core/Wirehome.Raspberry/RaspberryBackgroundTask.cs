using System;
using Windows.ApplicationModel.Background;
using Wirehome.Contracts.Core;

namespace Wirehome.Raspberry.Core
{
    public class RaspberryBackgroundTask : INativeBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        public RaspberryBackgroundTask(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance?.GetDeferral() ?? throw new ArgumentNullException(nameof(taskInstance));
        }

        public void Complete()
        {
            _deferral?.Complete();
        }
    }
}
