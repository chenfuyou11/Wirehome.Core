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
using Wirehome.Core.Hardware.RemoteSockets;
using Wirehome.ComponentModel.Extensions;
using Wirehome.ComponentModel.Capabilities.Constants;

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
            base.Initialize();

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
            // save registred and forward all
            //_state[message.Message.Pin] = await UpdateState(TemperatureState.StateName, _state[message.Message.Pin], message.Message.Temperature);
        }

        protected async Task TurnOnCommandHandler(Command message)
        {
            var system = message[CommandProperties.System].ToStringValue();
            var unit = message[CommandProperties.Unit].ToStringValue();
            var repeat = GetPropertyValue(CommandProperties.Repeat, new IntValue(DEFAULT_REPEAT)).Value.ToIntValue();
            var code = DipswitchCode.ParseCode(system, unit, RemoteSocketCommand.TurnOn);

            await _eventAggregator.Publish(new RemoteSocketMessage(code, (byte)_I2cAddress.Value, (byte)_pinNumber.Value, (byte)repeat.Value), RoutingFilter.MessageWrite);

            // TODO do we have all info?
            var commandCode = $"{system}|{unit}";
            _state[commandCode] = await UpdateState(PowerState.StateName, _state.ElementAtOrNull(commandCode), new StringValue(PowerStateValue.ON));
        }

        protected async Task TurnOffCommandHandler(Command message)
        {
            var system = message[CommandProperties.System].ToStringValue();
            var unit = message[CommandProperties.Unit].ToStringValue();
            var repeat = GetPropertyValue(CommandProperties.Repeat, new IntValue(DEFAULT_REPEAT)).Value.ToIntValue();
            var code = DipswitchCode.ParseCode(system, unit, RemoteSocketCommand.TurnOff);

            await _eventAggregator.Publish(new RemoteSocketMessage(code, (byte)_I2cAddress.Value, (byte)_pinNumber.Value, (byte)repeat.Value), RoutingFilter.MessageWrite);
            
            var commandCode = $"{system}|{unit}";
            
            _state[commandCode] = await UpdateState(PowerState.StateName, _state.ElementAtOrNull(commandCode), new StringValue(PowerStateValue.OFF));
        }

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState());
        }
    }
}