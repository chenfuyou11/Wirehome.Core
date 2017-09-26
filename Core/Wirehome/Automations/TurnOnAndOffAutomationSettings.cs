using System;
using Wirehome.Contracts.Automations;

namespace Wirehome.Automations
{
    public class TurnOnAndOffAutomationSettings : AutomationSettings
    {
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(60);
    }
}
