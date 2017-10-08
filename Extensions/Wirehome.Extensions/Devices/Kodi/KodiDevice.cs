using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wirehome.Components;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Components.States;
using Wirehome.Extensions.Devices.Commands;
using Wirehome.Extensions.Devices.Computer;
using Wirehome.Extensions.Devices.Features;
using Wirehome.Extensions.Devices.States;
using Wirehome.Extensions.Exceptions;
using Wirehome.Extensions.Messaging.ComputerMessages;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Messaging.KodiMessages;
using Wirehome.Extensions.Messaging.StateChangeMessages;
using Wirehome.Extensions.Quartz;

namespace Wirehome.Extensions.Devices.Kodi
{
    public class KodiDevice : DeviceComponent, IDisposable
    {
        private PowerStateValue _powerState;
        private float _volume;
        private bool _mute;
        private string _input;
        private TimeSpan _statusInterval { get; set; } = TimeSpan.FromSeconds(3);
        private readonly CancellationTokenSource _cancelationTokenSource = new CancellationTokenSource();
        private string _hostname;
        private int _port;
        private string _userName;
        private string _Password;

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

        public int Port
        {
            get => _port;
            set
            {
                if (_isInitialized) throw new PropertySetAfterInitializationExcption();
                _port = value;
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

        public string UserName
        {
            get => _userName;
            set
            {
                if (_isInitialized) throw new PropertySetAfterInitializationExcption();
                _userName = value;
            }
        }

        public string Password
        {
            get => _Password;
            set
            {
                if (_isInitialized) throw new PropertySetAfterInitializationExcption();
                _Password = value;
            }
        }

        public KodiDevice(string id, IEventAggregator eventAggregator, IScheduler scheduler) : base(id, eventAggregator)
        {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public async Task Initialize()
        {
            InitPowerStateFeature();
            InitVolumeFeature();
            InitMuteFeature();
            InitInputSourceFeature();

            var context = new KodiStateJobContext
            {
                Hostname = Hostname
            };

            // TODO silent errors when check status wile turned off
            await _scheduler.ScheduleIntervalWithContext<KodiStateJob, KodiStateJobContext>(StatusInterval, context, _cancelationTokenSource.Token).ConfigureAwait(false);
            await _scheduler.Start().ConfigureAwait(false);

            //TODO Get some state info

            _isInitialized = true;
        }

        public void Dispose()
        {
            _cancelationTokenSource.Cancel();
            // _eventAggregator.UnSubscribe(_statusSubscription);
            _scheduler.Shutdown();
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
                await _eventAggregator.PublishWithResultAsync<ComputerControlMessage, string>(new ComputerControlMessage
                {
                    Address = Hostname,
                    Service = "Process",
                    Message = new ProcessPost { ProcessName = "kodi", Start = true },
                    Port = 5000
                }).ConfigureAwait(false);
            }
           );
            _commandExecutor.Register<TurnOffCommand>(async c =>


            {
                var result = await _eventAggregator.PublishWithResultAsync<KodiMessage, string>(new KodiMessage
                {
                    Address = Hostname,
                    UserName = UserName,
                    Password = Password,
                    Port = Port,
                    Method = "JSONRPC.Ping"
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

               

                SetVolumeState(volume);
            }
          );
            _commandExecutor.Register<VolumeDownCommand>(async c =>
            {
                if (c == null) throw new ArgumentNullException();
                var volume = _volume - c.DefaultChangeFactor;

               

                SetVolumeState(volume);
            }
            );

            _commandExecutor.Register<SetVolumeCommand>(async c =>
            {
                if (c == null) throw new ArgumentNullException();

               

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
                

                SetMuteState(true);
            });

            _commandExecutor.Register<MuteOffCommand>(async c =>
            {
              

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
