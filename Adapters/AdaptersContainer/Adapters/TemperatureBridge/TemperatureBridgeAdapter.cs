using System.Threading.Tasks;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Extensions;
using Wirehome.Extensions.Messaging;
using System.Collections.Generic;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.Services;
using Wirehome.Model.ComponentModel.Capabilities.Constants;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class TemperatureBridgeAdapter : Adapter
    {
        private readonly ISerialMessagingService _serialMessagingService;
        private Dictionary<IntValue, DoubleValue> _state = new Dictionary<IntValue, DoubleValue>();

        public TemperatureBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            _serialMessagingService = adapterServiceFactory.GetUartService();
            _requierdProperties.Add(AdapterProperties.PinNumber);
        }

        public override async Task Initialize()
        {
            base.Initialize();

            var _i2cAddress = Properties[AdapterProperties.I2cAddress].Value.ToIntValue();

            foreach(var val in Properties[AdapterProperties.UsedPins].Value.ToStringList())
            {
                var pin = IntValue.FromString(val);
                await _eventAggregator.Publish(new TemperatureMessage(pin, _i2cAddress));
                _state.Add(pin, 0);
            }
            
            _serialMessagingService.RegisterBinaryMessage(TemperatureMessage.Empty);
            _disposables.Add(_eventAggregator.SubscribeAsync<TemperatureMessage>(TemperatureChangeHandler, RoutingFilter.MessageRead));
        }
        
        public async Task TemperatureChangeHandler(IMessageEnvelope<TemperatureMessage> message)
        {
            _state[message.Message.Pin] = await UpdateState(TemperatureState.StateName, _state[message.Message.Pin], message.Message.Temperature);
        }
        
        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new TemperatureState(ReadWriteModeValues.Read));
        }
    }
}