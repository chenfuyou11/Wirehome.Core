using Wirehome.Contracts.Components.Attributes;
using Wirehome.Contracts.Components.States;
using Wirehome.Extensions.Devices.Features;

namespace Wirehome.Extensions.Devices.States
{
    [Feature(typeof(MuteFeature))]
    public class MuteState : SimpleValueState<bool>
    {
        public MuteState(bool value) : base(value)
        {
        }
    }

}
