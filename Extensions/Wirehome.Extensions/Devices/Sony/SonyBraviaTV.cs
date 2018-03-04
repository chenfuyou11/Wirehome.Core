using Quartz;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wirehome.Components;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Components.States;
using Wirehome.Core.EventAggregator;
using Wirehome.Extensions.Devices.Commands;
using Wirehome.Extensions.Devices.Features;
using Wirehome.Extensions.Devices.States;
using Wirehome.Extensions.Exceptions;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Messaging.SonyMessages;
using Wirehome.Extensions.Messaging.StateChangeMessages;
using Wirehome.Extensions.Quartz;

namespace Wirehome.Extensions.Devices.Sony
{
    // TODO test when power off
    public class SonyBraviaTV : DeviceComponent, IDisposable
    {
        private PowerStateValue _powerState;
        private float _volume;
        private bool _mute;
        private string _input;
        private TimeSpan _statusInterval { get; set; } = TimeSpan.FromSeconds(3);
        private readonly CancellationTokenSource _cancelationTokenSource = new CancellationTokenSource();
        private string _hostname;
        private string _authorisationKey;

        public Dictionary<string, string> _inputSourceMap = new Dictionary<string, string>
        {
            { "HDMI1", "AAAAAgAAABoAAABaAw==" },
            { "HDMI2", "AAAAAgAAABoAAABbAw==" },
            { "HDMI3", "AAAAAgAAABoAAABcAw==" },
            { "HDMI4", "AAAAAgAAABoAAABdAw==" }
        };
        private readonly IScheduler _scheduler;

        public string Hostname
        {
            get => _hostname;
            set
            {
                if (_isInitialized) throw new PropertySetAfterInitializationExcption();
                _hostname = value;
            }
        }

        public string AuthorisationKey
        {
            get => _authorisationKey;
            set
            {
                if (_isInitialized) throw new PropertySetAfterInitializationExcption();
                _authorisationKey = value;
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

        public SonyBraviaTV(string id, IEventAggregator eventAggregator, IScheduler scheduler) : base(id, eventAggregator) {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public async Task Initialize()
        {
            InitPowerStateFeature();
            InitVolumeFeature();
            InitMuteFeature();
            InitInputSourceFeature();

            var context = new SonyStateJobContext
            {
                Hostname = Hostname,
                AuthKey = AuthorisationKey
            };

            // TODO silent errors when check status wile turned off
            await _scheduler.ScheduleIntervalWithContext<SonyStateJob, SonyStateJobContext>(StatusInterval, context, _cancelationTokenSource.Token).ConfigureAwait(false);
            await _scheduler.Start().ConfigureAwait(false);

            //TODO Get some state info

            _isInitialized = true;
        }

        public void Dispose()
        {
            _cancelationTokenSource.Cancel();
           // _eventAggregator.UnSubscribe(_statusSubscription);

        }

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection().With(new PowerState(_powerState))
                                                               .With(new MuteState(_mute))
                                                               .With(new VolumeState(_volume))
                                                               .With(new InputSourceState(_input));
        }

        #region Power Feature
        private void InitPowerStateFeature()
        {
            _featuresSupported.With(new PowerStateFeature());

            _commandExecutor.Register<TurnOnCommand>(async c =>
            {
                var result = await _eventAggregator.QueryAsync<SonyControlMessage, string>(new SonyControlMessage
                {
                    Address = Hostname,
                    AuthorisationKey = AuthorisationKey,
                    Code = "AAAAAQAAAAEAAAAuAw=="
                }).ConfigureAwait(false);
            }
           );
            _commandExecutor.Register<TurnOffCommand>(async c =>
            {
                var result = await _eventAggregator.QueryAsync<SonyControlMessage, string>(new SonyControlMessage
                {
                    Address = Hostname,
                    AuthorisationKey = AuthorisationKey,
                    Code = "AAAAAQAAAAEAAAAvAw=="
                }).ConfigureAwait(false);
            }
            );
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

                await _eventAggregator.QueryAsync<SonyJsonMessage, string>(new SonyJsonMessage
                {
                    Address = Hostname,
                    AuthorisationKey = AuthorisationKey,
                    Path = "audio",
                    Method = "setAudioVolume",
                    Params = new SonyAudioVolumeRequest("speaker", ((int)volume).ToString())
                }).ConfigureAwait(false);

                SetVolumeState(volume);
            }
          );
            _commandExecutor.Register<VolumeDownCommand>(async c =>
            {
                if (c == null) throw new ArgumentNullException();
                var volume = _volume - c.DefaultChangeFactor;

                await _eventAggregator.QueryAsync<SonyJsonMessage, string>(new SonyJsonMessage
                {
                    Address = Hostname,
                    AuthorisationKey = AuthorisationKey,
                    Path = "audio",
                    Method = "setAudioVolume",
                    Params = new SonyAudioVolumeRequest("speaker", ((int)volume).ToString())
                }).ConfigureAwait(false);

                SetVolumeState(volume);
            }
            );

            _commandExecutor.Register<SetVolumeCommand>(async c =>
            {
                if (c == null) throw new ArgumentNullException();
                
                await _eventAggregator.QueryAsync<SonyJsonMessage, string>(new SonyJsonMessage
                {
                    Address = Hostname,
                    AuthorisationKey = AuthorisationKey,
                    Path = "audio",
                    Method = "setAudioVolume",
                    Params = new SonyAudioVolumeRequest("speaker", ((int)c.Volume).ToString())
                }).ConfigureAwait(false);

                SetVolumeState(c.Volume);
            });
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
                await _eventAggregator.QueryAsync<SonyJsonMessage, string>(new SonyJsonMessage
                {
                    Address = Hostname,
                    AuthorisationKey = AuthorisationKey,
                    Path = "audio",
                    Method = "setAudioMute",
                    Params = new SonyAudioMuteRequest(true)
                }).ConfigureAwait(false);

                SetMuteState(true);
            });

            _commandExecutor.Register<MuteOffCommand>(async c =>
            {
                await _eventAggregator.QueryAsync<SonyJsonMessage, string>(new SonyJsonMessage
                {
                    Address = Hostname,
                    AuthorisationKey = AuthorisationKey,
                    Path = "audio",
                    Method = "setAudioMute",
                    Params = new SonyAudioMuteRequest(false)
                }).ConfigureAwait(false);

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

                if(!_inputSourceMap.ContainsKey(c.InputName)) throw new Exception($"Input {c.InputName} was not found on available device input sources");

                var cmd = _inputSourceMap[c.InputName];

                var result = await _eventAggregator.QueryAsync<SonyControlMessage, string>(new SonyControlMessage
                {
                    Address = Hostname,
                    AuthorisationKey = AuthorisationKey,
                    Code = cmd
                }).ConfigureAwait(false);

                SetInputSource(c.InputName);
            });
        }

        private void SetInputSource(string input)
        {
            if (_input == input) { return; }

            _eventAggregator.Publish(new InpuSourceChangeMessage(Id, new InputSourceState(_input), new InputSourceState(input)));
            _input = input;
        }
        #endregion
    }
}
