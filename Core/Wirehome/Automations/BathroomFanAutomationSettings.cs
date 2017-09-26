using System;
using Wirehome.Contracts.Automations;

namespace Wirehome.Automations
{
    public class BathroomFanAutomationSettings : AutomationSettings
    {
        public TimeSpan SlowDuration { get; set; } = TimeSpan.FromMinutes(8);
        public TimeSpan FastDuration { get; set; } = TimeSpan.FromMinutes(12);
    }
}
