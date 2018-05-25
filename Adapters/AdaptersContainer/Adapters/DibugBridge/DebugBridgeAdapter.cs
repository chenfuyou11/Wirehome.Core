using System.Threading.Tasks;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Services;
using Wirehome.Extensions.Messaging;
using Wirehome.Model.ComponentModel.Capabilities.Constants;
using Wirehome.Model.Extensions;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class DebugBridgeAdapter : Adapter
    {
        private readonly ISerialMessagingService _serialMessagingService;
        

        public DebugBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            _serialMessagingService = adapterServiceFactory.GetUartService();
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);

            var _i2cAddress = Properties[AdapterProperties.I2cAddress].Value.ToIntValue();

            _serialMessagingService.RegisterBinaryMessage(DebugMessage.Empty);
            _disposables.Add(_eventAggregator.SubscribeAsync<DebugMessage>(DebugChangeHandler, RoutingFilter.MessageRead));
        }

        public Task DebugChangeHandler(IMessageEnvelope<DebugMessage> message) => _eventAggregator.Publish(message.Message);
        
        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new CurrentState(ReadWriteModeValues.Read));
        }
    }
}