using Wirehome.Contracts.Components.Attributes;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Extensions.Devices.States;

namespace Wirehome.Extensions.Devices.Commands
{
    [FeatureState(typeof(PlaybackState))]
    public class PlayCommand : ICommand
    {
    }
}
