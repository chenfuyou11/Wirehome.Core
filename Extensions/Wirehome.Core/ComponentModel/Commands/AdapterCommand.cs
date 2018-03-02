

using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel.Commands
{
    public class DeviceCommand : Command
    {
        public DeviceCommand(string commandType, string deviceUid)
        {
            Type = commandType;
            this[CommandProperties.DeviceUid] = (StringValue)deviceUid;
        }
    }

    

}
