using Wirehome.Contracts.Components.Attributes;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Extensions.Devices.States;

namespace Wirehome.Extensions.Devices.Commands
{
    [FeatureState(typeof(InputSourceState))]
    public class ChangeInputSourceCommand : ICommand
    {
        public ChangeInputSourceCommand(string inputName)
        {
            InputName = inputName;
        }

        public string InputName { get; }
    }
}
