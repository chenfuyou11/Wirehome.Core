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
using Wirehome.Model.Extensions;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class HumidityBridgeAdapter : Adapter
    {
        private readonly ISerialMessagingService _serialMessagingService;
        private Dictionary<IntValue, DoubleValue> _state = new Dictionary<IntValue, DoubleValue>();

        public HumidityBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            _serialMessagingService = adapterServiceFactory.GetUartService();
            _requierdProperties.Add(AdapterProperties.PinNumber);
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);

            var _i2cAddress = Properties[AdapterProperties.I2cAddress].Value.ToIntValue();

            foreach(var val in Properties[AdapterProperties.UsedPins].Value.ToStringList())
            {
                var pin = IntValue.FromString(val);
                await _eventAggregator.Publish(new HumidityMessage(pin, _i2cAddress));
                _state.Add(pin, 0);
            }
            
            _serialMessagingService.RegisterBinaryMessage(HumidityMessage.Empty);
            _disposables.Add(_eventAggregator.SubscribeAsync<HumidityMessage>(HumidityChangeHandler, RoutingFilter.MessageRead));
        }
        
        public async Task HumidityChangeHandler(IMessageEnvelope<HumidityMessage> message)
        {
            _state[message.Message.Pin] = await UpdateState(HumidityState.StateName, _state[message.Message.Pin], message.Message.Humidity);
        }
        
        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new HumidityState(ReadWriteModeValues.Read));
        }
    }
}