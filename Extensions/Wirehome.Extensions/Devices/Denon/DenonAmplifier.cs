using System;
using System.Threading.Tasks;
using System.Threading;
using Quartz;
using Wirehome.Components;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Extensions.Messaging.DenonMessages;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Messaging.StateChangeMessages;
using Wirehome.Extensions.Devices.Features;
using Wirehome.Extensions.Devices.Commands;
using Wirehome.Extensions.Devices.States;
using Wirehome.Extensions.Quartz;
using Wirehome.Extensions.Extensions;
using Wirehome.Extensions.Devices.Denon;
using Wirehome.Extensions.Core;
using Wirehome.Extensions.Exceptions;
using Wirehome.Core.EventAggregator;

namespace Wirehome.Extensions.Devices
{
    // TODO: Check what happens when denon is turn off
    public class DenonAmplifier : DeviceComponent, IDisposable
    {
        private PowerStateValue _powerState;
        private float _volume;
        private bool _mute;
        private string _input;
        private string _surround;
        private DenonDeviceInfo _fullState;
        private string _hostname;
        private int _zone = 1;
        private TimeSpan _statusInterval { get; set; } = TimeSpan.FromSeconds(3);
        private readonly IScheduler _scheduler;
        private readonly CancellationTokenSource _cancelationTokenSource = new CancellationTokenSource();
        private SubscriptionToken _statusSubscription;
        
        public string Hostname
        {
            get => _hostname;
            set
            {
                if (_isInitialized) throw new PropertySetAfterInitializationExcption();
                _hostname = value;
            }
        }

        public int Zone
        {
            get => _zone;
            set
            {
                if (_isInitialized) throw new PropertySetAfterInitializationExcption();
                _zone = value;
            }
        }

        public TimeSpan StatusInterval
        {
            get => _statusInterval;
            set
            {
                if (_isInitialized) throw new PropertySetAfterInitializationExcption();
                _statusInterval = value;
            }
        }

        public DenonAmplifier(string id, IEventAggregator eventAggregator, IScheduler scheduler) : base(id, eventAggregator)
        {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public async Task Initialize()
        {
            InitPowerStateFeature();
            InitVolumeFeature();
            InitMuteFeature();
            InitInputSourceFeature();
            InitSurroundFeature();

            _statusSubscription = _eventAggregator.Subscribe<DenonStatus>(DenonStatusChanged);

            var context = new DenonStateJobContext
            {
                Hostname = Hostname,
                Zone = Zone.ToString()
            };
            await _scheduler.ScheduleIntervalWithContext<DenonStateJob, DenonStateJobContext>(StatusInterval, context, _cancelationTokenSource.Token).ConfigureAwait(false);
            await _scheduler.Start().ConfigureAwait(false);
            await RefreshDeviceState().ConfigureAwait(false);

            _isInitialized = true;
        }

        public async Task RefreshDeviceState()
        {
            _fullState = await _eventAggregator.QueryAsync<DenonStatusMessage, DenonDeviceInfo>(new DenonStatusMessage { Address = Hostname }).ConfigureAwait(false);
            var mapping = await _eventAggregator.QueryAsync<DenonMappingMessage, DenonDeviceInfo>(new DenonMappingMessage { Address = Hostname }).ConfigureAwait(false);
            _fullState.FriendlyName = mapping.FriendlyName;
            _fullState.InputMap = mapping.InputMap;
            _surround = _fullState.Surround;
        }

        public void DenonStatusChanged(IMessageEnvelope<DenonStatus> status)
        {
            SetPowerState(status.Message.PowerStatus);
            SetVolumeState(status.Message.MasterVolume);
            SetInputSource(status.Message.ActiveInput);
            SetMuteState(status.Message.Mute);
        }

        public void Dispose()
        {
            _cancelationTokenSource.Cancel();
            _statusSubscription.Dispose();
            _scheduler.Shutdown();
        }

        public override IComponentFeatureStateCollection GetState()
        {
            var states = new ComponentFeatureStateCollection().With(new PowerState(_powerState))
                                                              .With(new MuteState(_mute))
                                                              .With(new VolumeState(_volume))
                                                              .With(new InputSourceState(_input));

            if (Zone == 1)
            {
               states.With(new SurroundState(_surround));
            }

            return states;
        }

        #region Power Feature
        private void InitPowerStateFeature()
        {
            _featuresSupported.With(new PowerStateFeature());
            _commandExecutor.Register<TurnOnCommand>(async c =>
            {
                await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
                {
                    Command = "PowerOn",
                    Api = "formiPhoneAppPower",
                    ReturnNode = "Power",
                    Address = Hostname,
                    Zone = Zone.ToString()
                }, "ON").ConfigureAwait(false);
                SetPowerState(PowerStateValue.On);
            });
            _commandExecutor.Register<TurnOffCommand>(async c =>
            {
                await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
                {
                    Command = "PowerStandby",
                    Api = "formiPhoneAppPower",
                    ReturnNode = "Power",
                    Address = Hostname,
                    Zone = Zone.ToString()
                }, "OFF").ConfigureAwait(false);
                SetPowerState(PowerStateValue.Off);
            }
            );
        }
        private void SetPowerState(PowerStateValue powerState)
        {
            if (_powerState == powerState) { return; }
            _eventAggregator.Publish(new PowerStateChangeMessage(Id, new PowerState(_powerState), new PowerState(powerState)));
            _powerState = powerState;
        }
        #endregion

        #region Volume Feature
        private void InitVolumeFeature()
        {
            _featuresSupported.With(new VolumeFeature());
            _commandExecutor.Register<VolumeUpCommand>(async c =>
            {
                if (c == null) throw new ArgumentNullException();
                var volume = _volume + c.DefaultChangeFactor;
                var normalized = NormalizeVolume(volume);

                // Results are unpredictyble so we ignore them
                await _eventAggregator.QueryAsync<DenonControlMessage, string>(new DenonControlMessage
                {
                    Command = normalized,
                    Api = "formiPhoneAppVolume",
                    ReturnNode = "MasterVolume",
                    Address = Hostname,
                    Zone = Zone.ToString()
                }).ConfigureAwait(false);

                SetVolumeState(volume);
            });
            _commandExecutor.Register<VolumeDownCommand>(async c =>
            {
                if (c == null) throw new ArgumentNullException();
                var volume = _volume - c.DefaultChangeFactor;
                var normalized = NormalizeVolume(volume);

                await _eventAggregator.QueryAsync<DenonControlMessage, string>(new DenonControlMessage
                {
                    Command = normalized,
                    Api = "formiPhoneAppVolume",
                    ReturnNode = "MasterVolume",
                    Address = Hostname,
                    Zone = Zone.ToString()
                }).ConfigureAwait(false);

                SetVolumeState(volume);
            });
            _commandExecutor.Register<SetVolumeCommand>(async c =>
            {
                if (c == null) throw new ArgumentNullException();
                var normalized = NormalizeVolume(c.Volume);

                await _eventAggregator.QueryAsync<DenonControlMessage, string>(new DenonControlMessage
                {
                    Command = normalized,
                    Api = "formiPhoneAppVolume",
                    ReturnNode = "MasterVolume",
                    Address = Hostname,
                    Zone = Zone.ToString()
                }).ConfigureAwait(false);

                SetVolumeState(c.Volume);
            });
        }

        public string NormalizeVolume(float volume)
        {
            if (volume < 0) volume = 0;
            if (volume > 100) volume = 100;

            return (volume - 80).ToFloatString();
        }

        private void SetVolumeState(float? volume)
        {
            if (_volume == volume) { return; }

            _eventAggregator.Publish(new VolumeStateChangeMessage(Id, new VolumeState(_volume), new VolumeState(volume)));
            _volume = volume.GetValueOrDefault();
        }

        #endregion

        #region Mute Feature
        private void InitMuteFeature()
        {
            _featuresSupported.With(new MuteFeature());

            _commandExecutor.Register<MuteOnCommand>(async c =>
            {
                await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
                {
                    Command = "MuteOn",
                    Api = "formiPhoneAppMute",
                    ReturnNode = "Mute",
                    Address = Hostname,
                    Zone = Zone.ToString()
                }, "on").ConfigureAwait(false);

                SetMuteState(true);
            });

            _commandExecutor.Register<MuteOffCommand>(async c =>
            {
                await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
                {
                    Command = "MuteOff",
                    Api = "formiPhoneAppMute",
                    ReturnNode = "Mute",
                    Address = Hostname,
                    Zone = Zone.ToString()
                }, "off").ConfigureAwait(false);

                SetMuteState(false);
            });
        }

        private void SetMuteState(bool mute)
        {
            if (_mute == mute) { return; }

            _eventAggregator.Publish(new MuteStateChangeMessage(Id, new MuteState(_mute), new MuteState(mute)));
            _mute = mute;
        }
        #endregion

        #region Input Source Feature
        private void InitInputSourceFeature()
        {
            _featuresSupported.With(new InputSourceFeature());

            _commandExecutor.Register<ChangeInputSourceCommand>(async c =>
            {
                if (c == null) throw new ArgumentNullException();
                if (_fullState == null) throw new Exception("Cannot change input source on Denon device becouse device info was not downloaded from device");

                var input = _fullState.TranslateInputName(c.InputName, Zone.ToString());
                if(input?.Length == 0) throw new Exception($"Input {c.InputName} was not found on available device input sources");

                await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
                {
                    Command = input,
                    Api = "formiPhoneAppDirect",
                    ReturnNode = "",
                    Zone= "",
                    Address = Hostname
                }, "").ConfigureAwait(false);

                //TODO Check if this value is ok - confront with pooled state
                SetInputSource(input);
            });
        }

        private void SetInputSource(string input)
        {
            if (_input == input) { return; }

            _eventAggregator.Publish(new InpuSourceChangeMessage(Id, new InputSourceState(_input), new InputSourceState(input)));
            _input = input;
        }
        #endregion

        #region Surround Feature
        private void InitSurroundFeature()
        {
            // Surround support only in main zone
            if (Zone != 1) return;

            _featuresSupported.With(new SurroundModeFeature());

            _commandExecutor.Register<ChangeSurroundModeCommand>(async c =>
            {
                if (c == null) throw new ArgumentNullException();

                var mode = DenonSurroundModes.GetApiCommand(c.SurroundMode);
                if (mode?.Length == 0) throw new Exception($"Surroundmode {mode} was not found on available surround modes");

                await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
                {
                    Command = mode,
                    Api = "formiPhoneAppDirect",
                    ReturnNode = "",
                    Zone = "",
                    Address = Hostname
                }, "").ConfigureAwait(false);

                //TODO Check if this value is ok - confront with pooled state
                SetSurroundMode(mode);
            });
        }

        private void SetSurroundMode(string input)
        {
            if (_input == input) { return; }

            _eventAggregator.Publish(new SurroundChangeMessage(Id, new SurroundState(_input), new SurroundState(input)));
            _input = input;
        }
        #endregion
    }
}