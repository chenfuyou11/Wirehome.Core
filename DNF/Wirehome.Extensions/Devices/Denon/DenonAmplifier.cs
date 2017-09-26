using Wirehome.Components;
using System;
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
using Quartz;
using System.Threading.Tasks;
using System.Threading;
using Wirehome.Extensions.Quartz;

namespace Wirehome.Extensions.Devices
{

    public class DenonAmplifier : DeviceComponent, IDisposable
    {
        private PowerStateValue _powerState;
        private float _volume;
        private readonly IScheduler _scheduler;
        private CancellationTokenSource _cancelationTokenSource = new CancellationTokenSource();
        private Guid _statusSubscription;

        public string Hostname { get; set; }
        public int StatusInterval { get; set; } = 3;
        
        public DenonAmplifier(string id, string hostname, IEventAggregator eventAggregator, IScheduler scheduler) : base(id, eventAggregator)
        {
            Hostname = hostname ?? throw new ArgumentNullException(nameof(hostname));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public async Task Initialize()
        {
            InitPowerStateFeature();
            InitVolumeFeature();

            _statusSubscription = _eventAggregator.Subscribe<DenonStatus>(DenonStatusChanged);

            await _scheduler.ScheduleIntervalWithContext<DenonStateLightJob, string>(TimeSpan.FromSeconds(StatusInterval), Hostname, _cancelationTokenSource.Token);
            await _scheduler.Start();
        }

        public void DenonStatusChanged(IMessageEnvelope<DenonStatus> status)
        {

        }

        public void Dispose()
        {
            _cancelationTokenSource.Cancel();
            _eventAggregator.UnSubscribe(_statusSubscription);

            _scheduler.Shutdown();
        }

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection().With(new PowerState(_powerState));
        }

        #region Power Feature
        private void InitPowerStateFeature()
        {
            _featuresSupported.With(new PowerStateFeature());
            _commandExecutor.Register<TurnOnCommand>(async c =>
            {
                await _eventAggregator.PublishWithExpectedResultAsync(new DenonControlMessage
                {
                    Command = "PowerOn",
                    Api = "formiPhoneAppPower",
                    ReturnNode = "Power",
                    Address = Hostname
                }, "ON").ConfigureAwait(false);
                SetPowerState(PowerStateValue.On);
            });
            _commandExecutor.Register<TurnOffCommand>(async c =>
            {
                await _eventAggregator.PublishWithExpectedResultAsync(new DenonControlMessage
                {
                    Command = "PowerStandby",
                    Api = "formiPhoneAppPower",
                    ReturnNode = "Power",
                    Address = Hostname
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
                var state = await _eventAggregator.PublishWithExpectedResultAsync(new DenonControlMessage
                {
                    Command = "PowerOn",
                    Api = "formiPhoneAppPower",
                    ReturnNode = "Power",
                    Address = Hostname
                }, "ON").ConfigureAwait(false);
                //SetVolumeState(PowerStateValue.On);
            });
            _commandExecutor.Register<VolumeDownCommand>(async c =>
            {
                await _eventAggregator.PublishWithExpectedResultAsync(new DenonControlMessage
                {
                    Command = "PowerStandby",
                    Api = "formiPhoneAppPower",
                    ReturnNode = "Power",
                    Address = Hostname
                }, "OFF").ConfigureAwait(false);
                //SetVolumeState(PowerStateValue.Off);
            }
            );
        }
        private void SetVolumeState(float volume)
        {
            if (_volume == volume) { return; }
            _eventAggregator.Publish(new VolumeStateChangeMessage(Id, new VolumeState(_volume), new VolumeState(volume)));
            _volume = volume;
        }

        
        #endregion
    }
}
