using Wirehome.Contracts.Components;

namespace Wirehome.Actuators.Fans
{
    public class FanSettings : ComponentSettings
    {
        public int DefaultActiveLevel { get; set; } = 1;
    }
}
