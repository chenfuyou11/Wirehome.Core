using System;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core;

namespace Wirehome.ComponentModel.Events
{
    public class PropertyChangedEvent : Event
    {
        public PropertyChangedEvent(string propertyChangedName, IValue oldValue, IValue newValue)
        {
            Type = EventType.PropertyChanged;
            this[PropertyChangedEventProperty.Type] = (StringValue)propertyChangedName;
            this[PropertyChangedEventProperty.NewValue] = newValue;
            this[PropertyChangedEventProperty.OldValue] = oldValue;
            this[PropertyChangedEventProperty.EventTime] = (DateTimeValue)SystemTime.Now;
            //ComponentUid = componentUID;
        }
        public string PropertyChangedName => (StringValue)this[PropertyChangedEventProperty.Type];
        public IValue NewValue => this[PropertyChangedEventProperty.NewValue];
        public IValue OldValue => this[PropertyChangedEventProperty.OldValue];
        public DateTimeOffset EventTime => (DateTimeValue)this[PropertyChangedEventProperty.EventTime];
    }

    

}
