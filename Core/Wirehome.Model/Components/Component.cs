using CSharpFunctionalExtensions;
using Quartz;
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
using Wirehome.Model.Extensions;

namespace Wirehome.ComponentModel.Components
{
    public class Component : Actor
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;

        private List<string> _tagCache;
        private readonly ISchedulerFactory _schedulerFactory;

        private Dictionary<string, State> _capabilities { get; } = new Dictionary<string, State>();
        private Dictionary<string, AdapterReference> _adapterStateMap { get; } = new Dictionary<string, AdapterReference>();
        private Dictionary<string, AdapterReference> _eventSources { get; } = new Dictionary<string, AdapterReference>();
        [Map] private IList<AdapterReference> _adapters { get; set; } = new List<AdapterReference>();
        [Map] private IList<Trigger> _triggers { get; set; } = new List<Trigger>();
        [Map] private Dictionary<string, IValueConverter> _converters { get; set; } = new Dictionary<string, IValueConverter>();


        public Component(IEventAggregator eventAggregator, ILogService logService, ISchedulerFactory schedulerFactory)
        {
            _eventAggregator = eventAggregator;
            _logger = logService.CreatePublisher($"Component_{Uid}_logger");
            _schedulerFactory = schedulerFactory;
        }

        public override async Task Initialize()
        {
            if (!IsEnabled) return;
            await InitializeAdapters().ConfigureAwait(false);
            await InitializeTriggers().ConfigureAwait(false);

            await base.Initialize().ConfigureAwait(false);
        }

        //TODO Add conditions
        private Task InitializeTriggers()
        {
            foreach (var trigger in _triggers.Where(x => x.Schedule == null))
            {
                _disposables.Add(_eventAggregator.SubscribeForDeviceEvent(DeviceTriggerHandler, trigger.Event.GetPropertiesStrings(), trigger.Event.Type));
            }

            foreach (var trigger in _triggers.Where(x => x.Schedule != null))
            {

            }

            return Task.CompletedTask;
           // var scheduler = await _schedulerFactory.GetScheduler();
            //await scheduler.ScheduleIntervalWithContext
            //await scheduler.ScheduleIntervalWithContext<T, Adapter>(interval, this, _disposables.Token);
            //await scheduler.Start(_disposables.Token);
        }

        private async Task InitializeAdapters()
        {
            foreach (var adapter in _adapters)
            {
                var capabilities = await _eventAggregator.QueryDeviceAsync<DiscoveryResponse>(new DeviceCommand(CommandType.DiscoverCapabilities, adapter.Uid)).ConfigureAwait(false);
                if (capabilities == null) throw new Exception($"Failed to initialize adapter {adapter.Uid} in component {Uid}. There is no response from DiscoveryResponse command");

                MapCapabilitiesToAdapters(adapter, capabilities.SupportedStates);
                BuildCapabilityStates(capabilities);
                MapEventSourcesToAdapters(adapter, capabilities.EventSources);
                SubscribeToAdapterEvents(adapter, capabilities.RequierdProperties);
            }
        }

        private void BuildCapabilityStates(DiscoveryResponse capabilities) =>
            _capabilities.AddRangeNewOnly(capabilities.SupportedStates.ToDictionary(key => ((StringValue)key[StateProperties.StateName]).ToString(), val => val));


        private void MapCapabilitiesToAdapters(AdapterReference adapter, State[] states) =>
            states.ForEach(state => _adapterStateMap[state[StateProperties.StateName].ToStringValue()] = adapter);


        private void MapEventSourcesToAdapters(AdapterReference adapter, IList<EventSource> eventSources) =>
            eventSources.ForEach(es => _eventSources[es[EventProperties.EventType].ToStringValue()] = adapter);

        private void SubscribeToAdapterEvents(AdapterReference adapter, IList<string> requierdProperties) =>
            _disposables.Add(_eventAggregator.SubscribeForDeviceEvent(DeviceEventHandler, GetAdapterRouterAttributes(adapter, requierdProperties)));

        private Dictionary<string, string> GetAdapterRouterAttributes(AdapterReference adapter, IList<string> requierdProperties)
        {
            var routerAttributes = new Dictionary<string, string>();
            foreach (var adapterProperty in requierdProperties)
            {
                if (!adapter.ContainsProperty(adapterProperty)) throw new Exception($"Adapter {adapter.Uid} in component {Uid} missing configuration property {adapterProperty}");
                routerAttributes.Add(adapterProperty, adapter[adapterProperty].ToString());
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
            bool handled = false;
            // TODO use value converter before publish and maybe queue?
            foreach (var state in _capabilities.Values.Where(capability => capability.IsCommandSupported(command)))
            {
                var adapter = _adapterStateMap[state[StateProperties.StateName].ToString()];
                await _eventAggregator.PublishDeviceCommnd(adapter.GetDeviceCommand(command)).ConfigureAwait(false);

                handled = true;
            }

            if (!handled)
            {
                return base.UnhandledCommand(command);
            }
            else
            {
                return VoidResult.Void;
            }
        }

        private async Task DeviceEventHandler(IMessageEnvelope<Event> deviceEvent)
        {
            var propertyName = (StringValue)deviceEvent.Message[StateProperties.StateName];
            if (!_capabilities.ContainsKey(propertyName)) return;

            var state = _capabilities[propertyName];
            var oldValue = state[StateProperties.Value];
            var newValue = deviceEvent.Message[EventProperties.NewValue];

            if (newValue.Equals(oldValue)) return;

            state[StateProperties.Value] = newValue;

            await _eventAggregator.PublishDeviceEvent(new PropertyChangedEvent(Uid, propertyName, oldValue, newValue)).ConfigureAwait(false);
        }

        private async Task DeviceTriggerHandler(IMessageEnvelope<Event> deviceEvent)
        {
            var trigger = _triggers.FirstOrDefault(t => t.Event.Equals(deviceEvent.Message));
            if (trigger != null)
            {
                await ExecuteCommand(trigger.Command).ConfigureAwait(false);
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