using Wirehome.Core;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class DenonStatus
    {
        public string ActiveInput { get; set; }
        public BinaryState PowerStatus { get; set; }
        public float? MasterVolume { get; set; }
        public bool Mute { get; set; }
    }
}