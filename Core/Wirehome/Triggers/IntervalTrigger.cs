using System;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Triggers;

namespace Wirehome.Triggers
{
    public class IntervalTrigger : Trigger
    {
        public IntervalTrigger(TimeSpan interval, ISchedulerService scheduleService)
        {
            if (scheduleService == null) throw new ArgumentNullException(nameof(scheduleService));

            scheduleService.Register("IntervalTrigger-" + Guid.NewGuid(), interval, () => Execute());
        }
    }
}
