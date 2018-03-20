using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.Events;
using Wirehome.ComponentModel.Extensions;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Extensions;
using Wirehome.Core.Services.DependencyInjection;

namespace Wirehome.ComponentModel.Components
{
    public sealed class Component : BaseObject, IService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly DisposeContainer _disposables = new DisposeContainer();
        private List<string> _tagCache;
        private Dictionary<string, State> _capabilities { get; } = new Dictionary<string, State>();
        [Map] private IList<AdapterReference> _adapters { get; set; } = new List<AdapterReference>();
        [Map] private IList<Trigger> _triggers { get; set; } = new List<Trigger>();
        [Map] private Dictionary<string, IValueConverter> _converters { get; set; } = new Dictionary<string, IValueConverter>();

        public bool IsEnabled { get; private set; }

        public Component(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public void Dispose() => _disposables.Dispose();

        public async Task Initialize()
        {
            foreach (var adapter in _adapters)
            {
                var adapterCapabilities = await _eventAggregator.QueryDeviceAsync<DeviceCommand, DiscoveryResponse>(new DeviceCommand(CommandType.DiscoverCapabilities, adapter.Uid));
                adapterCapabilities.SupportedStates.ForEach(state => state.SetAdapterReference(adapter));
                _capabilities.AddRangeNewOnly(adapterCapabilities.SupportedStates.ToDictionary(key => ((StringValue)key[StateProperties.StateName]).ToString(), val => val));

                var routerAttributes = GetAdapterRouterAttributes(adapter, adapterCapabilities.RequierdProperties);
                _disposables.Add(_eventAggregator.SubscribeForDeviceEvent(DeviceEventHandler, routerAttributes));
            }

            foreach(var trigger in _triggers)
            {
                _disposables.Add(_eventAggregator.SubscribeForDeviceEvent(DeviceTriggerHandler, trigger.Event.GetPropertiesStrings(), trigger.Event.Type));
            }
        }

        private Dictionary<string, string> GetAdapterRouterAttributes(AdapterReference adapter, IList<string> requierdProperties)
        {
            var routerAttributes = new Dictionary<string, string>();
            foreach (var adapterProperty in requierdProperties)
            {
                if (!adapter.Properties.ContainsKey(adapterProperty)) throw new Exception($"Adapter {adapter.Uid} in component {Uid} missing configuration property {adapterProperty}");
                routerAttributes.Add(adapterProperty, adapter.Properties[adapterProperty].ToString());
            }
            routerAttributes.Add(EventProperties.SourceDeviceUid, adapter.Uid);

            return routerAttributes;
        }

        public async Task ExecuteCommand(Command command)
        {
            // TODO use valueconverter before publish and maybe queue?
            foreach (var state in _capabilities.Values.Where(capability => capability.IsCommandSupported(command)))
            {
                await _eventAggregator.PublishDeviceCommnd(state.Adapter.GetDeviceCommand(command));
            }
        }

        private async Task DeviceEventHandler(IMessageEnvelope<Event> deviceEvent)
        {
            var propertyName = (StringValue)deviceEvent.Message[StateProperties.StateName];
            if (!_capabilities.ContainsKey(propertyName)) return;

            var state = _capabilities[propertyName];
            var oldValue = state.Properties[StateProperties.Value].Value;
            var newValue = deviceEvent.Message[StateProperties.Value];

            if (oldValue.Equals(newValue)) return;

            state.Properties[StateProperties.Value].Value = newValue;

            await _eventAggregator.PublishDeviceEvent(new PropertyChangedEvent(Uid, propertyName, oldValue, newValue));
        }

        private async Task DeviceTriggerHandler(IMessageEnvelope<Event> deviceEvent)
        {
            var trigger = _triggers.FirstOrDefault(t => t.Event.Equals(deviceEvent.Message));
            if (trigger != null)
            {
                await ExecuteCommand(trigger.Command);
            }
        }

        // TODO Change to message?
        public Maybe<IValue> GetStateValue(string stateName)
        {
            if (!_capabilities.ContainsKey(stateName)) return Maybe<IValue>.None;
            var value = _capabilities[stateName][StateProperties.Value];
            if (_converters.ContainsKey(stateName))
            {
                value = _converters[stateName].Convert(value);
            }
            return Maybe<IValue>.From(value);
        }

        public IReadOnlyList<string> AllTags
        {
            get
            {
                if (_tagCache == null)
                {
                    _tagCache = new List<string>(Tags);
                    _tagCache.AddRange(_capabilities.Values.SelectMany(x => x.Tags));
                }
                return _tagCache.AsReadOnly();
            }
        }

        public IEnumerable<string> SupportedCapabilities => _capabilities.Values
                                                                         .Select(cap => cap.GetPropertyValue(StateProperties.CapabilityName))
                                                                         .Where(cap => cap.HasValue)
                                                                         .Select(cap => cap.Value)
                                                                         .Cast<StringValue>()
                                                                         .Select(cap => cap.Value)
                                                                         .Distinct();

        public IEnumerable<string> SupportedStates => _capabilities.Values
                                                                   .Select(cap => cap.GetPropertyValue(StateProperties.StateName))
                                                                   .Where(cap => cap.HasValue)
                                                                   .Select(cap => cap.Value)
                                                                   .Cast<StringValue>()
                                                                   .Select(cap => cap.Value)
                                                                   .Distinct();
    }
}