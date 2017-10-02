using Wirehome.Contracts.Components.Attributes;
using Wirehome.Contracts.Components.States;
using Wirehome.Extensions.Devices.Features;

namespace Wirehome.Extensions.Devices.States
{
    [Feature(typeof(SurroundModeFeature))]
    public class SurroundState : SimpleValueState<string>
    {
        public SurroundState(string value) : base(value)
        {
        }
    }
}
