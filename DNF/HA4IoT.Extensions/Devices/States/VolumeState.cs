using HA4IoT.Contracts.Components.Attributes;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Extensions.Devices.Features;

namespace HA4IoT.Extensions.Devices.States
{
    [Feature(typeof(VolumeFeature))]
    public class VolumeState : NumericBasedState
    {
        public VolumeState(float? value) : base(value)
        {
        }
    }

}
