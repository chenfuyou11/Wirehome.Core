using System;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.Messaging;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Extensions;
using Wirehome.Core.Services.I2C;
using Wirehome.Extensions.Messaging;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class HardwareBridgeAdapter : Adapter
    {
        private readonly II2CBusService _i2cBusService;

        private I2CSlaveAddress _i2cAddress;

        public HardwareBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            _i2cBusService = adapterServiceFactory.GetI2CService();
        }

        public override async Task Initialize()
        {
            base.Initialize();

            _disposables.Add(_eventAggregator.Subscribe<IBinaryMessage>(HandleMessage));

            _i2cAddress = new I2CSlaveAddress(Properties[AdapterProperties.I2cAddress].Value.ToIntValue());
        }

        private void HandleMessage(IMessageEnvelope<IBinaryMessage> message)
        {
        }

        protected Task<object> DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState(),
                                                               new SurroundSoundState()
                                          ).ToTaskResult<object>();
        }

        protected async Task TurnOnCommandHandler(Command message)
        {
            var remoteSocket = new RemoteSocketMessage();
            _i2cBusService.Write(_i2cAddress, remoteSocket.Serialize());

            //_powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(true));
        }
    }
}