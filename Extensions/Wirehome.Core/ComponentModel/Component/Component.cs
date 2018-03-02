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

namespace Wirehome.ComponentModel.Component
{
    public sealed class Component : BaseObject, IService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly DisposeContainer _disposables = new DisposeContainer();
        private List<string> _tagCache;
        private Dictionary<string, State> _capabilities { get; } = new Dictionary<string, State>();
        private List<AdapterReference> _adapters { get; } = new List<AdapterReference>();
        private Dictionary<string, IValueConverter> _statePropertyConverters { get; } = new Dictionary<string, IValueConverter>();

        public bool IsEnabled { get; }
        
        
        public Component(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            // Subscribing for commands - this will allow to control command via EventAggregator
            //_disposables.Add(_eventAggregator.Subscribe<Command>(ExecuteCommand));
        }

        public void AddAdapter(AdapterReference adapterUid)
        {
            _adapters.Add(adapterUid);
        }

        public void Dispose() => _disposables.Dispose();

        public async Task Initialize()
        {
            foreach(var adapter in _adapters)
            {
                var adapterCapabilities = await _eventAggregator.QueryAsync<DeviceCommand, DiscoveryResponse>(new DeviceCommand(CommandType.DiscoverCapabilities, adapter.Uid), new DeviceFilter<DeviceCommand>()).ConfigureAwait(false);
                adapterCapabilities.SupportedStates.ForEach(state => state.SetAdapterReference(adapter));
                _capabilities.AddRangeNewOnly(adapterCapabilities.SupportedStates.ToDictionary(key => ((StringValue)key[StateProperties.StateName]).ToString(), val => val));
            }
        }

        public void RegisterPropertyConverter(string propertyName, IValueConverter valueConverter)
        {
            _statePropertyConverters.Add(propertyName, valueConverter);
        }

        public Maybe<IValue> GetStateValue(string stateName)
        {
            if (!_capabilities.ContainsKey(stateName)) return Maybe<IValue>.None;
            var value = _capabilities[stateName][StateProperties.Value];
            return Maybe<IValue>.From(_statePropertyConverters[stateName].Convert(value));
        }
        
        public async Task ExecuteCommand(Command command)
        {
            foreach(var state in _capabilities.Values.Where(c => ((StringListValue)c[StateProperties.SupportedCommands]).Value.Contains(command.Type)))
            {
                await _eventAggregator.Publish(state.Adapter.GetDeviceCommand(command)).ConfigureAwait(false);
            }
        }

        new public List<string> Tags
        {
            get
            {
                if (_tagCache == null)
                {
                    _tagCache = new List<string>(base.Tags);
                    _tagCache.AddRange(_capabilities.Values.SelectMany(x => x.Tags));
                }
                return _tagCache;
            }
        }
    }
}
