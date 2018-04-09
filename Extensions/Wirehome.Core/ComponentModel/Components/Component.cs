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
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Extensions;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Core.Services.Logging;

namespace Wirehome.ComponentModel.Components
{
    // TODO what about adapters that don't know the state (tv controled only by IR)
    public class Component : Actor
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;

        private List<string> _tagCache;
        private Dictionary<string, State> _capabilities { get; } = new Dictionary<string, State>();
        [Map] private IList<AdapterReference> _adapters { get; set; } = new List<AdapterReference>();
        [Map] private IList<Trigger> _triggers { get; set; } = new List<Trigger>();
        [Map] private Dictionary<string, IValueConverter> _converters { get; set; } = new Dictionary<string, IValueConverter>();

        public bool IsEnabled { get; private set; }

        public Component(IEventAggregator eventAggregator, ILogService logService)
        {
            _eventAggregator = eventAggregator;
            _logger = logService.CreatePublisher($"Component_{Uid}_logger");
        }

        public override async Task Initialize()
        {
            if (!IsEnabled) return;

            foreach (var adapter in _adapters)
            {
                var adapterCapabilities = await _eventAggregator.QueryDeviceAsync<DiscoveryResponse>(new DeviceCommand(CommandType.DiscoverCapabilities, adapter.Uid));
                adapterCapabilities.SupportedStates.ForEach(state => state.SetAdapterReference(adapter));
                _capabilities.AddRangeNewOnly(adapterCapabilities.SupportedStates.ToDictionary(key => ((StringValue)key[StateProperties.StateName]).ToString(), val => val));

                var routerAttributes = GetAdapterRouterAttributes(adapter, adapterCapabilities.RequierdProperties);
                _disposables.Add(_eventAggregator.SubscribeForDeviceEvent(DeviceEventHandler, routerAttributes));
            }

            foreach (var trigger in _triggers)
            {
                _disposables.Add(_eventAggregator.SubscribeForDeviceEvent(DeviceTriggerHandler, trigger.Event.GetPropertiesStrings(), trigger.Event.Type));
            }

            base.Initialize();
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

        protected override void LogException(Exception ex) => _logger.Error(ex, $"Unhanded component {Uid} exception");

        /// <summary>
        /// All command not handled by the component directly are routed to adapters
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected override async Task<object> UnhandledCommand(Command command)
        {
            // TODO use valueconverter before publish and maybe queue?
            foreach (var state in _capabilities.Values.Where(capability => capability.IsCommandSupported(command)))
            {
                await _eventAggregator.PublishDeviceCommnd(state.Adapter.GetDeviceCommand(command));
            }

            return null;
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

        protected IReadOnlyCollection<string> SupportedCapabilitiesCommandHandler(Command command) => _capabilities.Values
                                                                                                .Select(cap => cap.GetPropertyValue(StateProperties.StateName))
                                                                                                .Where(cap => cap.HasValue)
                                                                                                .Select(cap => cap.Value)
                                                                                                .Cast<StringValue>()
                                                                                                .Select(cap => cap.Value)
                                                                                                .Distinct()
                                                                                                .ToList()
                                                                                                .AsReadOnly();

        protected IReadOnlyCollection<string> SupportedStatesCommandHandler(Command command) => _capabilities.Values
                                                                                   .Select(cap => cap.GetPropertyValue(StateProperties.StateName))
                                                                                   .Where(cap => cap.HasValue)
                                                                                   .Select(cap => cap.Value)
                                                                                   .Cast<StringValue>()
                                                                                   .Select(cap => cap.Value)
                                                                                   .Distinct()
                                                                                   .ToList()
                                                                                   .AsReadOnly();

        protected IReadOnlyCollection<string> SupportedTagsCommandHandler(Command command)
        {
            if (_tagCache == null)
            {
                _tagCache = new List<string>(Tags);
                _tagCache.AddRange(_capabilities.Values.SelectMany(x => x.Tags));
            }
            return _tagCache.AsReadOnly();
        }

        protected Maybe<IValue> GetStateCommandHandler(Command command)
        {
            var stateName = command[CommandProperties.StateName].ToStringValue();

            if (!_capabilities.ContainsKey(stateName)) return Maybe<IValue>.None;
            var value = _capabilities[stateName][StateProperties.Value];
            if (_converters.ContainsKey(stateName))
            {
                value = _converters[stateName].Convert(value);
            }
            return Maybe<IValue>.From(value);
        }
    }
}