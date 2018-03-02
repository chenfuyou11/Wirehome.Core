using Wirehome.ComponentModel.Commands;

namespace Wirehome.Core.EventAggregator
{
    public class DeviceFilter<T> : MessageFilter where T : DeviceCommand
    {
        public override string GetCustomFilter(object message) => (message as T)?[CommandProperties.DeviceUid]?.ToString() ?? string.Empty;
    }
    
}
