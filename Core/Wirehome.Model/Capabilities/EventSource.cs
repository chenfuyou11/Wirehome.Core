using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Events;
using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel.Capabilities
{
    public class EventSource : BaseObject
    {
        public EventSource(string eventType)
        {
            this[EventProperties.EventTime] = new DateTimeValue();
            this[EventProperties.EventType] = new StringValue(eventType);
        }
        
    }

}
