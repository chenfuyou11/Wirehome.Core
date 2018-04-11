using Wirehome.Contracts.Components.Attributes;
using Wirehome.Contracts.Components.Features;

namespace Wirehome.Contracts.Components.States
{
    [Feature(typeof(PowerStateFeature))]
    public class PowerState : EnumBasedState<PowerStateValue>
    {
        public static readonly PowerState Off = new PowerState(PowerStateValue.Off);
        public static readonly PowerState On = new PowerState(PowerStateValue.On);

        public PowerState(PowerStateValue value) : base(value)
        {
        }
    }
}
