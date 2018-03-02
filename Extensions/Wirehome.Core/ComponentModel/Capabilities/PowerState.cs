using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel.Capabilities
{
    public class PowerState : State
    {
        public static string StateName { get; } = nameof(PowerState);

        public PowerState()
        {
            this[StateProperties.StateName] = new StringValue(nameof(PowerState));
            this[StateProperties.Value] = new StringValue(Constants.Capabilities.PowerController);
            this[StateProperties.Value] = new StringValue();
            this[StateProperties.ValueList] = new StringListValue(PowerStateValue.ON, PowerStateValue.OFF);
            this[StateProperties.SupportedCommands] = new StringListValue(CommandType.TurnOn, CommandType.TurnOff);
        }
    }

}
