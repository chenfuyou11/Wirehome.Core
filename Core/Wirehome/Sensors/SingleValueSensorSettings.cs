using Wirehome.Contracts.Components;

namespace Wirehome.Sensors
{
    public class SingleValueSensorSettings : ComponentSettings
    {
        public float MinDelta { get; set; } = 0.15F;
    }
}
