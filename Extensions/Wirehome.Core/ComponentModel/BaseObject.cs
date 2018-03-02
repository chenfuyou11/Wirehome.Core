using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Wirehome.ComponentModel.Events;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.Extensions;

namespace Wirehome.ComponentModel
{
    public class BaseObject
    {
        private readonly Subject<Event> _events = new Subject<Event>();
        private Dictionary<string, Property> _properties { get; } = new Dictionary<string, Property>();

        public IReadOnlyDictionary<string, Property> Properties => _properties.AsReadOnly();

        public string Uid { get; set; }
        public string Type { get; protected set; }
        public List<string> Tags { get; } = new List<string>();
        public IObservable<Event> Events => _events.AsObservable();
        protected bool SupressPropertyChangeEvent {get; set;}

        public IValue this[string propertyName]
        {
            get => GetPropertyValue(propertyName).Value ?? throw new KeyNotFoundException();
            set { SetPropertyValue(propertyName, value); }
        }

        public virtual Maybe<IValue> GetPropertyValue(string propertyName, IValue defaultValue = null)
        {
            var property = _properties[propertyName];
            if (property == null)
            {
                return defaultValue != null ? Maybe<IValue>.From(defaultValue) : Maybe<IValue>.None;
            }
            return Maybe<IValue>.From(property.Value);
        }

        public virtual void SetPropertyValue(string propertyName, IValue value)
        {
            Property property;
            if (!_properties.ContainsKey(propertyName))
            {
                property = new Property
                {
                    Type = propertyName
                };
                _properties.Add(propertyName, property);
            }
            else
            {
                property = _properties[propertyName];
            }
            var oldValue = property.Value;
            property.Value = value;

            if (SupressPropertyChangeEvent || value.Equals(oldValue)) return;

            _events.OnNext(new PropertyChangedEvent(property.Type, oldValue, value));
        }
    }

    

}
