using System;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.Extensions;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class DenonAdapter : Adapter
    {
        public const int DEFAULT_POOL_INTERVAL = 1000;

        private BooleanValue _powerState;
        private DoubleValue _volume;
        private BooleanValue _mute;
        private StringValue _input;
        private StringValue _surround;
        private DenonDeviceInfo _fullState;
        private string _hostName;
        private int _zone;
        private TimeSpan _poolInterval;

        public DenonAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        public override async Task Initialize()
        {
            base.Initialize();

            _hostName = Properties[AdapterProperties.Hostname].Value.ToStringValue();
            _poolInterval = GetPropertyValue(AdapterProperties.PoolInterval, new IntValue(DEFAULT_POOL_INTERVAL)).Value.ToTimeSpan();
            //TODO make zone as required parameter
            _zone = Properties[AdapterProperties.Zone].Value.ToIntValue();

            await ScheduleDeviceRefresh<RefreshLightStateJob>(_poolInterval);
            await ExecuteCommand(Command.RefreshCommand);
        }

        protected async Task RefreshCommandHandler(Command message)
        {
            _fullState = await _eventAggregator.QueryAsync<DenonStatusMessage, DenonDeviceInfo>(new DenonStatusMessage { Address = _hostName });
            var mapping = await _eventAggregator.QueryAsync<DenonMappingMessage, DenonDeviceInfo>(new DenonMappingMessage { Address = _hostName });
            _fullState.FriendlyName = mapping.FriendlyName;
            _fullState.InputMap = mapping.InputMap;
            _surround = _fullState.Surround;
        }

        protected async Task RefreshLightCommandHandler(Command message)
        {
            var state = await _eventAggregator.QueryAsync<DenonStatusLightMessage, DenonStatus>(new DenonStatusLightMessage
            {
                Address = _hostName,
                Zone = _zone.ToString()
            });

            _input = await UpdateState<StringValue>(InputSourceState.StateName, _input, state.ActiveInput);
            _volume = await UpdateState<DoubleValue>(VolumeState.StateName, _volume, state.MasterVolume);
            _mute = await UpdateState<BooleanValue>(MuteState.StateName, _mute, state.Mute);
            _powerState = await UpdateState<BooleanValue>(PowerState.StateName, _powerState, state.PowerStatus);
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
            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = "PowerOn",
                Api = "formiPhoneAppPower",
                ReturnNode = "Power",
                Address = _hostName,
                Zone = _zone.ToString()
            }, "ON");
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(true));
        }

        protected async Task TurnOffCommandHandler(Command message)
        {
            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = "PowerStandby",
                Api = "formiPhoneAppPower",
                ReturnNode = "Power",
                Address = _hostName,
                Zone = _zone.ToString()
            }, "OFF");
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(false));
        }

        protected async Task VolumeUpCommandHandler(Command command)
        {
            var volume = _volume + command[CommandProperties.ChangeFactor].ToDoubleValue();
            var normalized = NormalizeVolume(volume);

            // Results are unpredictyble so we ignore them
            await _eventAggregator.QueryAsync<DenonControlMessage, string>(new DenonControlMessage
            {
                Command = normalized,
                Api = "formiPhoneAppVolume",
                ReturnNode = "MasterVolume",
                Address = _hostName,
                Zone = _zone.ToString()
            });

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume));
        }

        protected async Task VolumeDownCommandHandler(Command command)
        {
            var volume = _volume - command[CommandProperties.ChangeFactor].ToDoubleValue();
            var normalized = NormalizeVolume(volume);

            await _eventAggregator.QueryAsync<DenonControlMessage, string>(new DenonControlMessage
            {
                Command = normalized,
                Api = "formiPhoneAppVolume",
                ReturnNode = "MasterVolume",
                Address = _hostName,
                Zone = _zone.ToString()
            });

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume));
        }

        protected async Task VolumeSetCommandHandler(Command command)
        {
            var volume = command[CommandProperties.Value].ToDoubleValue();
            var normalized = NormalizeVolume(volume);

            await _eventAggregator.QueryAsync<DenonControlMessage, string>(new DenonControlMessage
            {
                Command = normalized,
                Api = "formiPhoneAppVolume",
                ReturnNode = "MasterVolume",
                Address = _hostName,
                Zone = _zone.ToString()
            });

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume));
        }

        private string NormalizeVolume(double volume)
        {
            if (volume < 0) volume = 0;
            if (volume > 100) volume = 100;

            return (volume - 80).ToFloatString();
        }

        protected async Task MuteCommandHandler(Command message)
        {
            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = "MuteOn",
                Api = "formiPhoneAppMute",
                ReturnNode = "Mute",
                Address = _hostName,
                Zone = _zone.ToString()
            }, "on");

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(true));
        }

        protected async Task UnmuteCommandHandler(Command message)
        {
            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = "MuteOff",
                Api = "formiPhoneAppMute",
                ReturnNode = "Mute",
                Address = _hostName,
                Zone = _zone.ToString()
            }, "off");

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(false));
        }

        protected async Task SelectInputCommandHandler(Command message)
        {
            if (_fullState == null) throw new Exception("Cannot change input source on Denon device becouse device info was not downloaded from device");
            var inputName = message[CommandProperties.InputSource].ToStringValue();
            var input = _fullState.TranslateInputName(inputName, _zone.ToString());
            if (input?.Length == 0) throw new Exception($"Input {inputName} was not found on available device input sources");

            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = input,
                Api = "formiPhoneAppDirect",
                ReturnNode = "",
                Zone = "",
                Address = _hostName
            }, "");

            //TODO Check if this value is ok - confront with pooled state
            _input = await UpdateState(InputSourceState.StateName, _input, inputName);
        }

        protected async Task SelectSurroundModeCommandHandler(Command message)
        {
            //Surround support only in main zone
            if (_zone != 1) return;
            var surroundMode = message[CommandProperties.SurroundMode].ToStringValue();
            var mode = DenonSurroundModes.MapApiCommand(surroundMode);
            if (mode?.Length == 0) throw new Exception($"Surroundmode {mode} was not found on available surround modes");

            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = mode,
                Api = "formiPhoneAppDirect",
                ReturnNode = "",
                Zone = "",
                Address = _hostName
            }, "");

            //TODO Check if this value is ok - confront with pooled state
            _surround = await UpdateState(SurroundSoundState.StateName, _surround, surroundMode);
        }
    }
}