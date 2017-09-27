using Wirehome.Contracts.Components.Attributes;
using Wirehome.Contracts.Components.States;
using Wirehome.Extensions.Devices.Features;

namespace Wirehome.Extensions.Devices.States
{
    [Feature(typeof(VolumeFeature))]
    public class VolumeState : NumericBasedState
    {
        public VolumeState(float? value) : base(value)
        {
        }
    }

}
