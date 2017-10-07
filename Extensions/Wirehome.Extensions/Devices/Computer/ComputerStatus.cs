using Wirehome.Contracts.Components.States;

namespace Wirehome.Extensions.Devices
{
    public class ComputerStatus
    {
        public string ActiveInput { get; set; }
        public PowerStateValue PowerStatus { get; set; }
        public float? MasterVolume { get; set; }
        public bool Mute { get; set; }
    }
}
