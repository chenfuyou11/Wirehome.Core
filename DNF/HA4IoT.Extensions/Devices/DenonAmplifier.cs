using HA4IoT.Components;
using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.Commands;
using HA4IoT.Extensions.Messaging.DenonMessages;
using HA4IoT.Extensions.Messaging.Core;
using HA4IoT.Extensions.Messaging.StateChangeMessages;
using HA4IoT.Extensions.Devices.Features;
using HA4IoT.Extensions.Devices.Commands;
using HA4IoT.Extensions.Devices.States;
using Quartz;
using System.Threading.Tasks;
using System.Threading;
using HA4IoT.Extensions.Quartz;

namespace HA4IoT.Extensions.Devices
{
   

    public class DenonStateLightJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return null;
        }
    }

    public class DenonAmplifier : DeviceComponent, IDisposable
    {
        private PowerStateValue _powerState;
        private float _volume;
        private readonly IScheduler _scheduler;
        private CancellationTokenSource _cancelationTokenSource = new CancellationTokenSource();

        public string Hostname { get; set; }
        
        public DenonAmplifier(string id, string hostname, IEventAggregator eventAggregator, IScheduler scheduler) : base(id, eventAggregator)
        {
            Hostname = hostname ?? throw new ArgumentNullException(nameof(hostname));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            
            InitPowerStateFeature();
            InitVolumeFeature();
        }

        public async Task Init()
        {
            await _scheduler.ScheduleInterval<DenonStateLightJob>(TimeSpan.FromSeconds(5));
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

        public void Dispose()
        {
            _cancelationTokenSource.Cancel();
        }
        #endregion
    }
}
