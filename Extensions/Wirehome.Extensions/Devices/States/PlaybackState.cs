using Wirehome.Contracts.Components.Attributes;
using Wirehome.Contracts.Components.States;
using Wirehome.Extensions.Devices.Features;

namespace Wirehome.Extensions.Devices.States
{
    [Feature(typeof(PlaybackFeature))]
    public class PlaybackState : NumericBasedState
    {
        public PlaybackState(float? value) : base(value)
        {
        }
    }

}
