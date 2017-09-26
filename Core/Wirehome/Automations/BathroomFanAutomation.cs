using System;
using Wirehome.Components;
using Wirehome.Contracts.Actuators;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Sensors;
using Wirehome.Contracts.Sensors.Events;
using Wirehome.Contracts.Settings;
using Wirehome.Scheduling;
using Wirehome.Triggers;

namespace Wirehome.Automations
{
    public class BathroomFanAutomation : AutomationBase
    {
        private readonly IMessageBrokerService _messageBroker;
        private readonly ISchedulerService _schedulerService;
        
        private readonly IFan _fan;
        private IScheduledAction _delayedAction;

        public BathroomFanAutomation(string id, IFan fan, ISchedulerService schedulerService, ISettingsService settingsService, IMessageBrokerService messageBroker)
            : base(id)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            
            _fan = fan ?? throw new ArgumentNullException(nameof(fan));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));

            settingsService.CreateSettingsMonitor<BathroomFanAutomationSettings>(this, s => Settings = s.NewSettings);
        }

        public BathroomFanAutomationSettings Settings { get; private set; }

        public BathroomFanAutomation WithTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            _messageBroker.CreateTrigger<MotionDetectedEvent>(motionDetector.Id).Attach(TurnOnSlow);
            _messageBroker.CreateTrigger<MotionDetectionCompletedEvent>(motionDetector.Id).Attach(StartTimeout);

            return this;
        }

        private void StartTimeout()
        {
            _delayedAction?.Cancel();

            _delayedAction = ScheduledAction.Schedule(Settings.SlowDuration, () =>
            {
                if (_fan.GetFeatures().Extract<LevelFeature>().MaxLevel > 1)
                {
                    _fan.TrySetLevel(2);
                    _delayedAction = ScheduledAction.Schedule(Settings.FastDuration, () => _fan.TryTurnOff());
                }
                else
                {
                    _fan.TryTurnOff();
                }
            });
        }

        private void TurnOnSlow()
        {
            if (!Settings.IsEnabled)
            {
                return;
            }

            _delayedAction?.Cancel();
            _fan.TrySetLevel(1);
        }
    }
}