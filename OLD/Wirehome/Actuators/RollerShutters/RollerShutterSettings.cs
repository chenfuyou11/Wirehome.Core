using System;
using Wirehome.Contracts.Components;

namespace Wirehome.Actuators.RollerShutters
{
    public class RollerShutterSettings : ComponentSettings
    {
        public int MaxPosition { get; set; } = 20000;

        public TimeSpan AutoOffTimeout { get; set; } = TimeSpan.FromSeconds(22);
    }
}
