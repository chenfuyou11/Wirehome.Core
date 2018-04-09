using System;
using System.Threading.Tasks;
using System.Threading;
using Quartz;
using Wirehome.Core.EventAggregator;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.Core.Extensions;
using Wirehome.ComponentModel.Messaging;

namespace Wirehome.ComponentModel.Adapters.Pc
{
    public class PcAdapter : Adapter
    {
        public const int DEFAULT_POOL_INTERVAL = 1000;

        private string _hostname;
        private int _port;
        private string _mac;
        private TimeSpan _poolInterval;

        private BooleanValue _powerState;
        private DoubleValue _volume;
        private BooleanValue _mute;
        private StringValue _input;

        public PcAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        public override Task Initialize()
        {
            base.Initialize();

            _hostname = Properties[AdapterProperties.Hostname].Value.ToStringValue();
            _port = Properties[AdapterProperties.Port].Value.ToIntValue();
            _mac = Properties[AdapterProperties.MAC].Value.ToStringValue();
            _poolInterval = GetPropertyValue(AdapterProperties.PoolInterval, new IntValue(DEFAULT_POOL_INTERVAL)).Value.ToTimeSpan();

            return ScheduleDeviceRefresh<RefreshStateJob>(_poolInterval);
        }

        protected async Task RefreshCommandHandler(Command message)
        {
            var state = await _eventAggregator.QueryAsync<ComputerControlMessage, ComputerStatus>(new ComputerControlMessage
            {
                Address = _hostname,
                Port = _port,
                Service = "Status",
                RequestType = "GET"
            });

            _input = await UpdateState<StringValue>(InputSourceState.StateName, _input, state.ActiveInput);
            _volume = await UpdateState<DoubleValue>(VolumeState.StateName, _volume, state.MasterVolume);
            _mute = await UpdateState<BooleanValue>(MuteState.StateName, _mute, state.Mute);
            _powerState = await UpdateState<BooleanValue>(PowerState.StateName, _powerState, state.PowerStatus);
        }

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState()
                                          );
        }

        protected async Task TurnOnCommandHandler(Command message)
        {
            await _eventAggregator.QueryAsync<WakeOnLanMessage, string>(new WakeOnLanMessage
            {
                MAC = _mac
            });
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(true));
        }

        protected async Task TurnOffCommandHandler(Command message)
        {
            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "Power",
                Message = new PowerPost { State = ComputerPowerState.Hibernate }
            });
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(false));
        }

        protected async Task VolumeUpCommandHandler(Command command)
        {
            var volume = _volume + command[CommandProperties.ChangeFactor].ToDoubleValue();

            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "Volume",
                Message = new VolumePost { Volume = volume }
            });

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume));
        }

        protected async Task VolumeDownCommandHandler(Command command)
        {
            var volume = _volume - command[CommandProperties.ChangeFactor].ToDoubleValue();
            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "Volume",
                Message = new VolumePost { Volume = volume }
            });

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume));
        }

        protected async Task VolumeSetCommandHandler(Command command)
        {
            var volume = command[CommandProperties.Value].ToDoubleValue();
            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "Volume",
                Message = new VolumePost { Volume = volume }
            });

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume));
        }

        protected async Task MuteCommandHandler(Command message)
        {
            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "Mute",
                Message = new MutePost { Mute = true }
            });

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(true));
        }

        protected async Task UnmuteCommandHandler(Command message)
        {
            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "Mute",
                Message = new MutePost { Mute = false }
            });

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(false));
        }

        protected async Task SelectInputCommandHandler(Command message)
        {
            var inputName = message[CommandProperties.InputSource].ToStringValue();

            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "InputSource",
                Message = new InputSourcePost { Input = inputName }
            });

            _input = await UpdateState(InputSourceState.StateName, _input, inputName);
        }
    }
}