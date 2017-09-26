using HA4IoT.Contracts.Components.Attributes;
using HA4IoT.Contracts.Components.States;

namespace HA4IoT.Contracts.Components.Commands
{
    [FeatureState(typeof(PowerState))]
    public class TurnOffCommand : ICommand
    {
    }
}
