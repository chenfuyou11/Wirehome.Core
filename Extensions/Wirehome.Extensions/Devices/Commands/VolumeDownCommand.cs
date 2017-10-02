using Wirehome.Contracts.Components.Attributes;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Extensions.Devices.States;


namespace Wirehome.Extensions.Devices.Commands
{
    [FeatureState(typeof(VolumeState))]
    public class VolumeDownCommand : ICommand
    {
        public float DefaultChangeFactor { get; set; } = 10.0f;
    }
}
