using System;
using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core;

namespace Wirehome.ComponentModel.Events
{
    public class PropertyChangedEvent : Event
    {
        public PropertyChangedEvent(string deviceUID, string changedPropertyName, IValue oldValue, IValue newValue)
        {
            Type = EventType.PropertyChanged;
            Uid = Guid.NewGuid().ToString();
            this[StateProperties.StateName] = (StringValue)changedPropertyName;
            this[EventProperties.SourceDeviceUid] = (StringValue)deviceUID;
            this[EventProperties.NewValue] = newValue;
            this[EventProperties.OldValue] = oldValue;
            this[EventProperties.EventTime] = (DateTimeValue)SystemTime.Now;
        }
        public string PropertyChangedName => (StringValue)this[StateProperties.StateName];
        public IValue NewValue => this[EventProperties.NewValue];
        public IValue OldValue => this[EventProperties.OldValue];
        public DateTimeOffset EventTime => (DateTimeValue)this[EventProperties.EventTime];
        public string SourceDeviceUid => (StringValue)this[EventProperties.SourceDeviceUid];
    }
}
