using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Extensions;
using Wirehome.Core.DI;
using Wirehome.ComponentModel.Events;
using Wirehome.ComponentModel.Extensions;
using System;

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
                var adapterCapabilities = await _eventAggregator.QueryDeviceAsync<DeviceCommand, DiscoveryResponse>(new DeviceCommand(CommandType.DiscoverCapabilities, adapter.Uid)).ConfigureAwait(false);
                adapterCapabilities.SupportedStates.ForEach(state => state.SetAdapterReference(adapter));
                _capabilities.AddRangeNewOnly(adapterCapabilities.SupportedStates.ToDictionary(key => ((StringValue)key[StateProperties.StateName]).ToString(), val => val));
                var routerAttributes = GetAdapterRouterAttributes(adapter, adapterCapabilities);

                _disposables.Add(_eventAggregator.SubscribeForDeviceEvent<Event>(DeviceEventHandler, adapter.Uid, routerAttributes));
            }
        }

        private Dictionary<string, string> GetAdapterRouterAttributes(AdapterReference adapter, DiscoveryResponse adapterCapabilities)
        {
            var routerAttributes = new Dictionary<string, string>();
            foreach (var adapterProperty in adapterCapabilities.RequierdProperties)
            {
                if (!adapter.Properties.ContainsKey(adapterProperty)) throw new Exception($"Adapter {adapter.Uid} in component {Uid} missing configuration property {adapterProperty}");
                routerAttributes.Add(adapterProperty, adapter.Properties[adapterProperty].ToString());
            }

            return routerAttributes;
        }

        public Task DeviceEventHandler(IMessageEnvelope<Event> message)
        {
            var deviceEvent = message.Message;
            var propertyName = (StringValue)deviceEvent[StateProperties.StateName];

            if(!_capabilities.ContainsKey(propertyName)) return Task.CompletedTask;

            var state = _capabilities[propertyName];
            state.Properties[StateProperties.Value].Value = deviceEvent[StateProperties.Value];
            //TODO add value change event

            return Task.CompletedTask;
        }

        public Maybe<IValue> GetStateValue(string stateName)
        {
            if (!_capabilities.ContainsKey(stateName)) return Maybe<IValue>.None;
            var value = _capabilities[stateName][StateProperties.Value];
            return Maybe<IValue>.From(_converters[stateName].Convert(value));
        }

        public async Task ExecuteCommand(Command command)
        {
            foreach (var state in _capabilities.Values.Where(capability => ((StringListValue)capability[StateProperties.SupportedCommands]).Value.Contains(command.Type)))
            {
                await _eventAggregator.Publish(state.Adapter.GetDeviceCommand(command)).ConfigureAwait(false);
            }
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
