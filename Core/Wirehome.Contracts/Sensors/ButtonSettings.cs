using System;
using Wirehome.Contracts.Components;

namespace Wirehome.Contracts.Sensors
{
    public class ButtonSettings : ComponentSettings
    {
        public TimeSpan PressedLongDuration { get; set; } = TimeSpan.FromSeconds(1.5);
    }
}
