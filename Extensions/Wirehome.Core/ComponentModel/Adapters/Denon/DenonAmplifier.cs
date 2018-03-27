using System.Threading.Tasks;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.Events;
using Wirehome.ComponentModel.Extensions;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Extensions;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class DenonAmplifier : Adapter
    {
        private bool _powerState;
        private double _volume;
        //private bool _mute;
        //private string _input;
        //private string _surround;
        //private DenonDeviceInfo _fullState;

        private IEventAggregator _eventAggregator;

        private string HostName { get; set; }
        private int Zone { get; set; }
        private int PoolInterval { get; set; }

        public DenonAmplifier(IAdapterServiceFactory adapterServiceFactory)
        {
            _eventAggregator = adapterServiceFactory.GetEventAggregator();
        }

        public async Task Initialize()
        {
            HostName = Properties["HostName"].Value as StringValue;
            Zone = Properties["Zone"].Value as IntValue;
            PoolInterval = Properties["PoolInterval"].Value as IntValue;

            //_statusSubscription = _eventAggregator.Subscribe<DenonStatus>(DenonStatusChanged);

            //var context = new DenonStateJobContext
            //{
            //    Hostname = Hostname,
            //    Zone = Zone.ToString()
            //};
            //await _scheduler.ScheduleIntervalWithContext<DenonStateJob, DenonStateJobContext>(StatusInterval, context, _cancelationTokenSource.Token).ConfigureAwait(false);
            //await _scheduler.Start().ConfigureAwait(false);
            //await RefreshDeviceState().ConfigureAwait(false);
        }

        //public async Task RefreshDeviceState()
        //{
        //    _fullState = await _eventAggregator.QueryAsync<DenonStatusMessage, DenonDeviceInfo>(new DenonStatusMessage { Address = Hostname }).ConfigureAwait(false);
        //    var mapping = await _eventAggregator.QueryAsync<DenonMappingMessage, DenonDeviceInfo>(new DenonMappingMessage { Address = Hostname }).ConfigureAwait(false);
        //    _fullState.FriendlyName = mapping.FriendlyName;
        //    _fullState.InputMap = mapping.InputMap;
        //    _surround = _fullState.Surround;
        //}

        protected Task<object> DiscoverCapabilitiesHandler(Command message) => new DiscoveryResponse(RequierdProperties(), new PowerState(), new VolumeState()).ToStaticTaskResult();

        protected async Task TurnOnCommandHandler(Command message)
        {
            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = "PowerOn",
                Api = "formiPhoneAppPower",
                ReturnNode = "Power",
                Address = HostName,
                Zone = Zone.ToString()
            }, "ON");
            await SetPowerState(true);
        }

        protected async Task TurnOffCommandHandler(Command message)
        {
            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = "PowerStandby",
                Api = "formiPhoneAppPower",
                ReturnNode = "Power",
                Address = HostName,
                Zone = Zone.ToString()
            }, "OFF").ConfigureAwait(false);
            await SetPowerState(false);
        }

        private async Task SetPowerState(bool powerState)
        {
            if (_powerState == powerState) { return; }
            var properyChangeEvent = new PropertyChangedEvent(Uid, PowerState.StateName, new BooleanValue(_powerState), new BooleanValue(powerState));
            await _eventAggregator.PublishDeviceEvent(properyChangeEvent, _requierdProperties);
            _powerState = powerState;
        }

        protected async Task VolumeUpCommandHandler(Command command)
        {
            var volume = _volume + command["ChangeFactor"].ToDoubleValue();
            var normalized = NormalizeVolume(volume);

            // Results are unpredictyble so we ignore them
            await _eventAggregator.QueryAsync<DenonControlMessage, string>(new DenonControlMessage
            {
                Command = normalized,
                Api = "formiPhoneAppVolume",
                ReturnNode = "MasterVolume",
                Address = HostName,
                Zone = Zone.ToString()
            }).ConfigureAwait(false);

            await SetVolumeState(volume);
        }

        protected async Task VolumeDownCommandHandler(Command command)
        {
            var volume = _volume - command["ChangeFactor"].ToDoubleValue();
            var normalized = NormalizeVolume(volume);

            await _eventAggregator.QueryAsync<DenonControlMessage, string>(new DenonControlMessage
            {
                Command = normalized,
                Api = "formiPhoneAppVolume",
                ReturnNode = "MasterVolume",
                Address = HostName,
                Zone = Zone.ToString()
            }).ConfigureAwait(false);

            await SetVolumeState(volume);
        }

        protected async Task VolumeSetCommandHandler(Command command)
        {
            var volume = command["Value"].ToDoubleValue();
            var normalized = NormalizeVolume(volume);

            await _eventAggregator.QueryAsync<DenonControlMessage, string>(new DenonControlMessage
            {
                Command = normalized,
                Api = "formiPhoneAppVolume",
                ReturnNode = "MasterVolume",
                Address = HostName,
                Zone = Zone.ToString()
            }).ConfigureAwait(false);

            await SetVolumeState(volume);
        }

        public string NormalizeVolume(double volume)
        {
            if (volume < 0) volume = 0;
            if (volume > 100) volume = 100;

            return (volume - 80).ToFloatString();
        }

        private async Task SetVolumeState(double? volume)
        {
            if (_volume == volume) { return; }
            await _eventAggregator.PublishDeviceEvent(new PropertyChangedEvent(Uid, VolumeState.StateName, new DoubleValue(_volume), new DoubleValue(_volume)), _requierdProperties);
            _volume = volume.GetValueOrDefault();
        }

        //#region Mute Feature
        //private void InitMuteFeature()
        //{
        //    _featuresSupported.With(new MuteFeature());

        //    _commandExecutor.Register<MuteOnCommand>(async c =>
        //    {
        //        await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
        //        {
        //            Command = "MuteOn",
        //            Api = "formiPhoneAppMute",
        //            ReturnNode = "Mute",
        //            Address = Hostname,
        //            Zone = Zone.ToString()
        //        }, "on").ConfigureAwait(false);

        //        SetMuteState(true);
        //    });

        //    _commandExecutor.Register<MuteOffCommand>(async c =>
        //    {
        //        await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
        //        {
        //            Command = "MuteOff",
        //            Api = "formiPhoneAppMute",
        //            ReturnNode = "Mute",
        //            Address = Hostname,
        //            Zone = Zone.ToString()
        //        }, "off").ConfigureAwait(false);

        //        SetMuteState(false);
        //    });
        //}

        //private void SetMuteState(bool mute)
        //{
        //    if (_mute == mute) { return; }

        //    _eventAggregator.Publish(new MuteStateChangeMessage(Id, new MuteState(_mute), new MuteState(mute)));
        //    _mute = mute;
        //}
        //#endregion

        //#region Input Source Feature
        //private void InitInputSourceFeature()
        //{
        //    _featuresSupported.With(new InputSourceFeature());

        //    _commandExecutor.Register<ChangeInputSourceCommand>(async c =>
        //    {
        //        if (c == null) throw new ArgumentNullException();
        //        if (_fullState == null) throw new Exception("Cannot change input source on Denon device becouse device info was not downloaded from device");

        //        var input = _fullState.TranslateInputName(c.InputName, Zone.ToString());
        //        if(input?.Length == 0) throw new Exception($"Input {c.InputName} was not found on available device input sources");

        //        await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
        //        {
        //            Command = input,
        //            Api = "formiPhoneAppDirect",
        //            ReturnNode = "",
        //            Zone= "",
        //            Address = Hostname
        //        }, "").ConfigureAwait(false);

        //        //TODO Check if this value is ok - confront with pooled state
        //        SetInputSource(input);
        //    });
        //}

        //private void SetInputSource(string input)
        //{
        //    if (_input == input) { return; }

        //    _eventAggregator.Publish(new InpuSourceChangeMessage(Id, new InputSourceState(_input), new InputSourceState(input)));
        //    _input = input;
        //}
        //#endregion

        //#region Surround Feature
        //private void InitSurroundFeature()
        //{
        //    // Surround support only in main zone
        //    if (Zone != 1) return;

        //    _featuresSupported.With(new SurroundModeFeature());

        //    _commandExecutor.Register<ChangeSurroundModeCommand>(async c =>
        //    {
        //        if (c == null) throw new ArgumentNullException();

        //        var mode = DenonSurroundModes.GetApiCommand(c.SurroundMode);
        //        if (mode?.Length == 0) throw new Exception($"Surroundmode {mode} was not found on available surround modes");

        //        await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
        //        {
        //            Command = mode,
        //            Api = "formiPhoneAppDirect",
        //            ReturnNode = "",
        //            Zone = "",
        //            Address = Hostname
        //        }, "").ConfigureAwait(false);

        //        //TODO Check if this value is ok - confront with pooled state
        //        SetSurroundMode(mode);
        //    });
        //}

        //private void SetSurroundMode(string input)
        //{
        //    if (_input == input) { return; }

        //    _eventAggregator.Publish(new SurroundChangeMessage(Id, new SurroundState(_input), new SurroundState(input)));
        //    _input = input;
        //}
        //#endregion
    }
}