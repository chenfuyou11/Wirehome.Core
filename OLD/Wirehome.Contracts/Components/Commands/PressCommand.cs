using Wirehome.Contracts.Sensors;

namespace Wirehome.Contracts.Components.Commands
{
    public class PressCommand : ICommand
    {
        public ButtonPressedDuration Duration { get; set; } = ButtonPressedDuration.Short;
    }
}
