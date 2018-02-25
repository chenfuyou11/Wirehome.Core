using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Wirehome.Core.Constants;
using Wirehome.Core.EventAggregator;

namespace Wirehome.Core
{
    public class BaseObject
    {
        private readonly Subject<Event> _events = new Subject<Event>();
        private Dictionary<string, Property> Properties { get; } = new Dictionary<string, Property>();

        public string Uid { get; }
        public string Type { get; protected set; }
        public List<string> Tags { get; } = new List<string>();
        public IObservable<Event> Events => _events.AsObservable();

        public IValue this[string propertyName]
        {
            get => GetPropertyValue(propertyName).Value ?? throw new KeyNotFoundException();
            set { SetPropertyValue(propertyName, value); }
        }

        public Maybe<IValue> GetPropertyValue(string propertyName, IValue defaultValue = null)
        {
            var property = Properties[propertyName];
            if (property == null)
            {
                return defaultValue != null ? Maybe<IValue>.From(defaultValue) : Maybe<IValue>.None;
            }
            return Maybe<IValue>.From(property.Value);
        }

        public void SetPropertyValue(string propertyName, IValue value)
        {
            Property property;
            if (!Properties.ContainsKey(propertyName))
            {
                property = new Property
                {
                    Type = propertyName
                };
                Properties.Add(propertyName, property);
            }
            else
            {
                property = Properties[propertyName];
            }
            var oldValue = property.Value;
            property.Value = value;

            if (value.Equals(oldValue)) return;

            _events.OnNext(new PropertyChangedEvent(property.Type, oldValue, value));
        }
    }

    public sealed class Property : ValueObject
    {
        public string Type { get; set; }
        public IValue Value { get; set; }
        public override string ToString() => $"{Type}={Convert.ToString(Value)}";
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Type;
        }
    }

    public class Module : BaseObject
    {
    }
    
    public sealed class Component : BaseObject
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly DisposeContainer _disposables = new DisposeContainer();
        private List<string> _tagCache;

        
        public List<Module> Modules { get; } = new List<Module>();
        public bool IsEnabled { get; }

        public Component(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            // Subscribing for commands - this will allow to control command via EventAggregator
            //_disposables.Add(_eventAggregator.Subscribe<Command>(ExecuteCommand));
        }

        public void ExecuteCommand(Command command)
        {
        }

        new public List<string> Tags
        {
            get
            {
                if (_tagCache == null)
                {
                    _tagCache = new List<string>(base.Tags);
                    //_tagCache.AddRange(Modules.SelectMany(x => x.GetCapabilities()));
                }
                return _tagCache;
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }

    public class Command : BaseObject
    {
    }

    public class AdapterCommand : Command
    {
        public string AdapterUID { get; }

        public AdapterCommand(string adapterUID)
        {
            Type = CommandType.DeviceCommand;
            AdapterUID = adapterUID;
        }
    }


    public class Event : BaseObject
    {
    }

    
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
