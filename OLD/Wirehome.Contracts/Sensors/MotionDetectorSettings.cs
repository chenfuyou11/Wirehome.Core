using System;
using Wirehome.Contracts.Components;

namespace Wirehome.Contracts.Sensors
{
    public class MotionDetectorSettings : ComponentSettings
    {
        public TimeSpan AutoEnableAfter { get; set; } = TimeSpan.FromMinutes(60);
    }
}
