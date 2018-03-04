using System;
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
            this[PropertyChangedEventProperty.PropertyName] = (StringValue)changedPropertyName;
            this[PropertyChangedEventProperty.SourceDeviceUid] = (StringValue)deviceUID;
            this[PropertyChangedEventProperty.NewValue] = newValue;
            this[PropertyChangedEventProperty.OldValue] = oldValue;
            this[PropertyChangedEventProperty.EventTime] = (DateTimeValue)SystemTime.Now;
        }
        public string PropertyChangedName => (StringValue)this[PropertyChangedEventProperty.PropertyName];
        public IValue NewValue => this[PropertyChangedEventProperty.NewValue];
        public IValue OldValue => this[PropertyChangedEventProperty.OldValue];
        public DateTimeOffset EventTime => (DateTimeValue)this[PropertyChangedEventProperty.EventTime];
        public string SourceDeviceUid => (StringValue)this[PropertyChangedEventProperty.SourceDeviceUid];
    }
}
