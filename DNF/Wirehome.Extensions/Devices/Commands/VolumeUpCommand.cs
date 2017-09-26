using HA4IoT.Contracts.Components.Attributes;
using HA4IoT.Contracts.Components.Commands;
using HA4IoT.Extensions.Devices.States;

namespace HA4IoT.Extensions.Devices.Commands
{
    [FeatureState(typeof(VolumeState))]
    public class VolumeUpCommand : ICommand
    {
    }
}
