using Wirehome.Contracts.Components.Attributes;
using Wirehome.Contracts.Components.States;
using Wirehome.Extensions.Devices.Features;

namespace Wirehome.Extensions.Devices.States
{
    [Feature(typeof(InputSourceFeature))]
    public class InputSourceState  : SimpleValueState<string>
    {
        public InputSourceState(string value) : base(value)
        {
        }
    }
}
