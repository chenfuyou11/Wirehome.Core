using System.Collections.Generic;

namespace Wirehome.Contracts.Hardware.Interrupts.Configuration
{
    public class InterruptMonitorServiceConfiguration
    {
        public Dictionary<string, InterruptConfiguration> Interrupts { get; set; } = new Dictionary<string, InterruptConfiguration>();
    }
}
