using System.Collections.Generic;
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
using Wirehome.Core.Hardware.RemoteSockets;
using Wirehome.Core.Services;
using Wirehome.Extensions.Messaging;
using Wirehome.Model.Events;
using Wirehome.Model.Extensions;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class RemoteSocketBridgeAdapter : Adapter
    {
        private const int DEFAULT_REPEAT = 3;
        private IntValue _pinNumber;
        private IntValue _I2cAddress;

        private readonly ISerialMessagingService _serialMessagingService;
        private readonly Dictionary<StringValue, StringValue> _state = new Dictionary<StringValue, StringValue>();

        public RemoteSocketBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            _serialMessagingService = adapterServiceFactory.GetUartService();
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);

            _I2cAddress = Properties[AdapterProperties.I2cAddress].Value.ToIntValue();
            _pinNumber = Properties[AdapterProperties.PinNumber].Value.ToIntValue();

            _serialMessagingService.RegisterBinaryMessage(RemoteSocketMessage.Empty);
            _disposables.Add(_eventAggregator.SubscribeAsync<RemoteSocketMessage>(RemoteSocketChangeHandler, RoutingFilter.MessageRead));
            _disposables.Add(_eventAggregator.SubscribeForDeviceQuery<DeviceCommand>(DeviceCommandHandler, Uid));
        }

        private Task<object> DeviceCommandHandler(IMessageEnvelope<DeviceCommand> messageEnvelope)
        {
            return ExecuteCommand(messageEnvelope.Message);
        }

        public async Task RemoteSocketChangeHandler(IMessageEnvelope<RemoteSocketMessage> message)
        {
            var code = message.Message.DipswitchCode;

            if (_state.ContainsKey(code.ToShortCode()))
            {
                await UpdateState(code);
            }

            await _eventAggregator.PublishDeviceEvent(new DipswitchEvent(Uid, code));
        }

        protected async Task TurnOnCommandHandler(Command message)
        {
            var system = message[CommandProperties.System].ToStringValue();
            var unit = message[CommandProperties.Unit].ToStringValue();
            var repeat = GetPropertyValue(CommandProperties.Repeat, new IntValue(DEFAULT_REPEAT)).Value.ToIntValue();
            var code = DipswitchCode.ParseCode(system, unit, nameof(RemoteSocketCommand.TurnOn));

            await _eventAggregator.Publish(new RemoteSocketMessage(code, (byte)_I2cAddress.Value, (byte)_pinNumber.Value, (byte)repeat.Value), RoutingFilter.MessageWrite);
            await UpdateState(code);
        }

        private async Task UpdateState(DipswitchCode code)
        {
            _state[code.ToShortCode()] = await UpdateState(PowerState.StateName, _state.ElementAtOrNull(code.ToShortCode()), new StringValue(PowerStateValue.ON));
        }

        protected async Task TurnOffCommandHandler(Command message)
        {
            var system = message[CommandProperties.System].ToStringValue();
            var unit = message[CommandProperties.Unit].ToStringValue();
            var repeat = GetPropertyValue(CommandProperties.Repeat, new IntValue(DEFAULT_REPEAT)).Value.ToIntValue();
            var code = DipswitchCode.ParseCode(system, unit, nameof(RemoteSocketCommand.TurnOff));

            await _eventAggregator.Publish(new RemoteSocketMessage(code, (byte)_I2cAddress.Value, (byte)_pinNumber.Value, (byte)repeat.Value), RoutingFilter.MessageWrite);
            
            var commandCode = $"{system}|{unit}";
            _state[commandCode] = await UpdateState(PowerState.StateName, _state.ElementAtOrNull(commandCode), new StringValue(PowerStateValue.OFF));
        }

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(new List<EventSource> { new EventSource(EventType.DipswitchCode, EventDirections.Recieving),
                                                                 new EventSource(EventType.DipswitchCode, EventDirections.Sending)}, new PowerState());
        }
    }
}