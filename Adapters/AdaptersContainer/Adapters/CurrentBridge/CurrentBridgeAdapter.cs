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
    public class CurrentBridgeAdapter : Adapter
    {
        private readonly ISerialMessagingService _serialMessagingService;
        private Dictionary<IntValue, IntValue> _state = new Dictionary<IntValue, IntValue>();

        public CurrentBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
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
                await _eventAggregator.Publish(new CurrentMessage(pin, _i2cAddress));
                _state.Add(pin, 0);
            }
            
            _serialMessagingService.RegisterBinaryMessage(CurrentMessage.Empty);
            _disposables.Add(_eventAggregator.SubscribeAsync<CurrentMessage>(CurrentChangeHandler, RoutingFilter.MessageRead));
        }
        
        public async Task CurrentChangeHandler(IMessageEnvelope<CurrentMessage> message)
        {
            _state[message.Message.Pin] = await UpdateState(CurrentState.StateName, _state[message.Message.Pin], message.Message.Current);
        }
        
        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new CurrentState(ReadWriteModeValues.Read));
        }
    }
}