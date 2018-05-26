using System.Threading.Tasks;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.Core.Interface.Native;
using Wirehome.Core.Services;
using Wirehome.Core.Services.Logging;
using Wirehome.Model.ComponentModel.Capabilities.Constants;
using Wirehome.Model.Extensions;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class DebugBridgeAdapter : Adapter
    {
        private readonly ISerialMessagingService _serialMessagingService;
        private readonly ILogger _log;


        public DebugBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            _serialMessagingService = adapterServiceFactory.GetUartService();
            _log = adapterServiceFactory.GetLogger().CreatePublisher(nameof(DebugBridgeAdapter));
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);

            var _i2cAddress = Properties[AdapterProperties.I2cAddress].Value.ToIntValue();

            _serialMessagingService.RegisterMessageHandler(MessageHandler);
        }

        public Task<bool> MessageHandler(byte messageType, byte messageSize, IBinaryReader reader)
        {
            if (messageType == 10)
            {
                var debug = reader.ReadString(messageSize);
                _log.Error($"Debug message from { nameof(DebugBridgeAdapter) }: {debug}");

                return Task.FromResult(true);
            }
            return Task.FromResult(true);
        }

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new CurrentState(ReadWriteModeValues.Read));
        }
    }
}