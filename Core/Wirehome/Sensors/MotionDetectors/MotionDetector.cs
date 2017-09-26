using System;
using Wirehome.Components;
using Wirehome.Components.Commands;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Sensors;
using Wirehome.Contracts.Sensors.Events;
using Wirehome.Contracts.Settings;
using Wirehome.Scheduling;

namespace Wirehome.Sensors.MotionDetectors
{
    public class MotionDetector : ComponentBase, IMotionDetector
    {
        private readonly object _syncRoot = new object();
        private readonly IMessageBrokerService _messageBroker;

        private readonly CommandExecutor _commandExecutor = new CommandExecutor();

        private readonly ISettingsService _settingsService;
        private readonly ISchedulerService _schedulerService;

        private IScheduledAction _autoEnableAction;
        private MotionDetectionStateValue _motionDetectionState = MotionDetectionStateValue.Idle;

        public MotionDetector(string id, IMotionDetectorAdapter adapter, ISchedulerService schedulerService, ISettingsService settingsService, IMessageBrokerService messageBroker)
            : base(id)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));

            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));

            adapter.StateChanged += UpdateState;

            settingsService.CreateSettingsMonitor<MotionDetectorSettings>(this, s =>
            {
                Settings = s.NewSettings;

                if (s.OldSettings != null && s.OldSettings.IsEnabled != s.NewSettings.IsEnabled)
                {
                    HandleIsEnabledStateChanged();
                }
            });

            _commandExecutor.Register<ResetCommand>(c => adapter.Refresh());
        }

        public MotionDetectorSettings Settings { get; private set; }

        public override IComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection()
                .With(new MotionDetectionFeature());
        }

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(new MotionDetectionState(_motionDetectionState));
        }

        public override void ExecuteCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            lock (_syncRoot)
            {
                _commandExecutor.Execute(command);
            }
        }

        private void UpdateState(object sender, MotionDetectorAdapterStateChangedEventArgs e)
        {
            var state = e.State == AdapterMotionDetectionState.MotionDetected
                ? MotionDetectionStateValue.MotionDetected
                : MotionDetectionStateValue.Idle;

            lock (_syncRoot)
            {
                if (state == _motionDetectionState)
                {
                    return;
                }

                if (state == MotionDetectionStateValue.MotionDetected && !Settings.IsEnabled)
                {
                    return;
                }

                var oldState = GetState();
                _motionDetectionState = state;
                OnStateChanged(oldState);

                if (state == MotionDetectionStateValue.MotionDetected)
                {
                    _messageBroker.Publish(Id, new MotionDetectedEvent());
                }
                else if (state == MotionDetectionStateValue.Idle)
                {
                    _messageBroker.Publish(Id, new MotionDetectionCompletedEvent());
                }
            }
        }

        private void HandleIsEnabledStateChanged()
        {
            _autoEnableAction?.Cancel();

            if (!Settings.IsEnabled)
            {
                _autoEnableAction = ScheduledAction.Schedule(Settings.AutoEnableAfter, () => _settingsService.SetComponentEnabledState(this, true));
            }
        }
    }
}