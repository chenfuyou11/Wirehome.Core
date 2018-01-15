using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Collections.Generic;
using Wirehome.Conditions;
using Wirehome.Conditions.Specialized;
using Wirehome.Contracts.Conditions;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Components;
using System.Collections.ObjectModel;
using Wirehome.Motion.Model;
using Wirehome.Extensions;

namespace Wirehome.Motion
{
    //TODO Thread safe
    //TODO add change source in event to distinct the source of the change (manual light on or automatic)
    //TODO Manual turn off/on - react if was just after auto, code some automatic reaction (code from manual turn on/off)
    public class MotionDescriptor
    {
        private readonly ConditionsValidator _turnOnConditionsValidator = new ConditionsValidator();
        private readonly ConditionsValidator _turnOffConditionsValidator = new ConditionsValidator();
        private readonly IScheduler _scheduler;
        private readonly MotionConfiguration _motionConfiguration;

        internal IObservable<PowerStateValue> PowerChangeSource { get; } // TODO Add descriptor for some codes for change on/off

        // Configuration parameters
        public string MotionDetectorUid { get; }
        internal IEnumerable<string> Neighbors { get; }
        internal IReadOnlyCollection<MotionDescriptor> NeighborsCache { get; private set; }
        private IComponent Lamp { get; }
        private float LightIntensityAtNight { get; }

        // Dynamic parameters
        internal bool AutomationDisabled { get; private set; }
        internal int NumberOfPersonsInArea { get; private set; }
        internal DateTimeOffset? LastMotionTime { get; private set; }
        internal AreaDescriptor AreaDescriptor { get; }

        private TimeList _MotionHistory { get; }
        private Probability _PresenceProbability { get; set; } = Probability.Zero;
        private DateTimeOffset _AutomationEnableOn { get; set; }
        private DateTimeOffset _LastManualTurnOn { get; set; }
        private int _PresenseMotionCounter { get; set; }
        private MotionVector _LastEnter { get; set; }
        private MotionVector _LastLeave { get; set; }

        public override string ToString()
        {
            return $"{MotionDetectorUid} [Last move: {(LastMotionTime != null ? LastMotionTime?.Second.ToString() : "?")}:{(LastMotionTime != null ? LastMotionTime?.Millisecond.ToString() : "?")}] [Persons: {NumberOfPersonsInArea}]";
        }

        public MotionDescriptor(string motionDetectorUid, IEnumerable<string> neighbors, IComponent lamp, IScheduler scheduler,
                                IDaylightService daylightService, IDateTimeService dateTimeService, AreaDescriptor areaDescriptor,
                                MotionConfiguration motionConfiguration)
        {
            MotionDetectorUid = motionDetectorUid ?? throw new ArgumentNullException(nameof(motionDetectorUid));
            Neighbors = neighbors ?? throw new ArgumentNullException(nameof(neighbors));
            Lamp = lamp ?? throw new ArgumentNullException(nameof(lamp));

            if (areaDescriptor.WorkingTime == WorkingTime.DayLight)
            {
                _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsDayCondition(daylightService, dateTimeService));
            }
            else if (areaDescriptor.WorkingTime == WorkingTime.AfterDusk)
            {
                _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsNightCondition(daylightService, dateTimeService));
            }

            _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsEnabledAutomationCondition(this));
            _turnOffConditionsValidator.WithCondition(ConditionRelation.And, new IsEnabledAutomationCondition(this));

            _MotionHistory = new TimeList(scheduler);

            PowerChangeSource = Lamp.ToPowerChangeSource();

            _scheduler = scheduler;
            _motionConfiguration = motionConfiguration;
            AreaDescriptor = areaDescriptor;
        }

        public void MarkMotion(DateTimeOffset time)
        {
            LastMotionTime = time;
            _MotionHistory.Add(time);
            _PresenseMotionCounter++;
            SetProbability(Probability.Full);
        }

        public void Update()
        {
            CheckForTurnOnAutomationAgain();

            RecalculateProbability();
        }

        public void MarkEnter(MotionVector vector)
        {
            _LastEnter = vector;
            NumberOfPersonsInArea++;
        }

        public void MarkLeave(MotionVector vector)
        {
            _LastLeave = vector;
            if (NumberOfPersonsInArea > 0)
            {
                NumberOfPersonsInArea--;
            }

            if (AreaDescriptor.MaxPersonCapacity == 1)
            {
                SetProbability(Probability.Zero);
            }
            else
            {
                //TODO change this value
                SetProbability(Probability.FromValue(0.1));
            }
        }

        internal void BuildNeighborsCache(IEnumerable<MotionDescriptor> neighbors) => NeighborsCache = new ReadOnlyCollection<MotionDescriptor>(neighbors.ToList());
        internal void DisableAutomation() => AutomationDisabled = true;
        internal void EnableAutomation() => AutomationDisabled = false;
        internal void DisableAutomation(TimeSpan time)
        {
            DisableAutomation();
            _AutomationEnableOn = _scheduler.Now + time;
        }

        private void RecalculateProbability()
        {
            var probabilityDelta = 1.0 / (AreaDescriptor.TurnOffTimeout.Ticks / _motionConfiguration.PeriodicCheckTime.Ticks);

            SetProbability(_PresenceProbability.Decrease(probabilityDelta));
        }

        private void CheckForTurnOnAutomationAgain()
        {
            if (AutomationDisabled && _scheduler.Now > _AutomationEnableOn)
            {
                EnableAutomation();
            }
        }

        private void ResetStatistics()
        {
            NumberOfPersonsInArea = 0;
            _MotionHistory.ClearOldData(AreaDescriptor.MotionDetectorAlarmTime);
        }

        private void SetProbability(Probability probability)
        {
            _PresenceProbability = probability;

            if(_PresenceProbability.IsFullProbability)
            {
                TryTurnOnLamp();
            }
            else if(_PresenceProbability.IsNoProbability)
            {
                TryTurnOffLamp();
            }
        }

        private void TryTurnOnLamp()
        {
            if (CanTurnOnLamp()) Lamp.ExecuteCommand(new TurnOnCommand());
        }

        private void TryTurnOffLamp()
        {
            if (CanTurnOffLamp())
            {
                Lamp.ExecuteCommand(new TurnOffCommand());
                ResetStatistics();
            }
        }

        private bool CanTurnOnLamp() => _turnOnConditionsValidator.Validate() != ConditionState.NotFulfilled;
        private bool CanTurnOffLamp() => _turnOffConditionsValidator.Validate() != ConditionState.NotFulfilled;
    }

}