using Wirehome.Contracts.Components.Attributes;
using Wirehome.Contracts.Components.States;

namespace Wirehome.Contracts.Components.Commands
{
    [FeatureState(typeof(PowerState))]
    public class TurnOnCommand : ICommand
    {
    }
}
