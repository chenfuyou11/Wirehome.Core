using Wirehome.Contracts.Components.Attributes;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Extensions.Devices.States;

namespace Wirehome.Extensions.Devices.Commands
{
    [FeatureState(typeof(SurroundState))]
    public class ChangeSurroundModeCommand : ICommand
    {
        public ChangeSurroundModeCommand(string surroundMode)
        {
            SurroundMode = surroundMode;
        }

        public string SurroundMode { get; }
    }
}
