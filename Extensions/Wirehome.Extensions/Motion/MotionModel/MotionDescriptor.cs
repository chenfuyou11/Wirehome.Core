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

namespace Wirehome.Extensions.MotionModel
{
    //TODO Thread safe
    //TODO add change source in event to distinct the source of the change (manual light on or automatic)
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
        internal TimeSpan MotionDetectorAlarmTime { get; }
        private IComponent Lamp { get; }
        private float LightIntensityAtNight { get; }
        private int MaxPersonCapacity { get; }
        private AreaType AreaType { get; }
        private TimeSpan TurnOffTimeout { get; }

        // Dynamic parameters
        internal bool AutomationDisabled { get; private set; }
        internal int NumberOfPersonsInArea { get; private set; }
        internal DateTimeOffset? LastMotionTime { get; private set; }

        private TimeList _MotionHistory { get; }
        private double _PresenceProbability { get; set; }
        private DateTimeOffset _AutomationEnableOn { get; set; }
        private DateTimeOffset _LastManualTurnOn { get; set; }
        private int _PresenseMotionCounter { get; set; }
        private MotionVector _LastEnter { get; set; }
        private MotionVector _LastLeave { get; set; }
        
        public override string ToString()
        {
            return $"{MotionDetectorUid} [Last move: {(LastMotionTime != null ? LastMotionTime?.Second.ToString() : "?")}:{(LastMotionTime != null ? LastMotionTime?.Millisecond.ToString() : "?")}]";
        }


        // TODO
        // Add pending turnoff source - we could turn off but it was not sure last time

        public MotionDescriptor(string motionDetectorUid, IEnumerable<string> neighbors, IComponent lamp, IScheduler scheduler,
                                IDaylightService daylightService, IDateTimeService dateTimeService, AreaDescriptor areaDescriptor, MotionConfiguration motionConfiguration)
        {
            MotionDetectorUid = motionDetectorUid ?? throw new ArgumentNullException(nameof(motionDetectorUid));
            Neighbors = neighbors ?? throw new ArgumentNullException(nameof(neighbors));
            Lamp = lamp ?? throw new ArgumentNullException(nameof(lamp));
            MaxPersonCapacity = areaDescriptor.MaxPersonCapacity;
            AreaType = areaDescriptor.AreaType;
            MotionDetectorAlarmTime = areaDescriptor.MotionDetectorAlarmTime;
            TurnOffTimeout = areaDescriptor.TurnOffTimeout;
            
            if (areaDescriptor.WorkingTime == WorkingTime.DayLight)
            {
                _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsDayCondition(daylightService, dateTimeService));
            }
            else if (areaDescriptor.WorkingTime == WorkingTime.AfterDusk)
            {
                _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsNightCondition(daylightService, dateTimeService));
            }

            _MotionHistory = new TimeList(scheduler);

            PowerChangeSource = Lamp.ToPowerChangeSource();
            _scheduler = scheduler;
            _motionConfiguration = motionConfiguration;
        }

        internal void BuildNeighborsCache(IEnumerable<MotionDescriptor> neighbors)
        {
            NeighborsCache = new ReadOnlyCollection<MotionDescriptor>(neighbors.ToList());
        }

        public void MarkMotion(DateTimeOffset time)
        {
            LastMotionTime = time;
            _MotionHistory.Add(time);
            _PresenseMotionCounter++;
            SetProbability(1.0);
        }
        
        public void Update()
        {
            CheckForTurnOnAutomationAgain();

            RecalculateProbability();
        }

        private void RecalculateProbability()
        {
            var probabilityDelta = 1.0 / (TurnOffTimeout.Ticks / _motionConfiguration.PeriodicCheckTime.Ticks);

            SetProbability(_PresenceProbability - probabilityDelta);
        }

        private void CheckForTurnOnAutomationAgain()
        {
            if (AutomationDisabled && _scheduler.Now > _AutomationEnableOn)
            {
                EnableAutomation();
            }
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

            if (MaxPersonCapacity == 1)
            {
                SetProbability(0);
            }
            else
            {
                SetProbability(0.1);
            }

        }

        private void ResetStatistics()
        {
            NumberOfPersonsInArea = 0;
            _MotionHistory.ClearOldData(MotionDetectorAlarmTime);
        }

        private void SetProbability(double probability)
        {
            if (probability < 0) probability = 0;
            _PresenceProbability = probability;


            if(_PresenceProbability == 1.0)
            {
                TryTurnOnLamp();
            }
            else if(_PresenceProbability == 0)
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
        
        private bool CanTurnOnLamp() => !(AutomationDisabled || (_turnOnConditionsValidator.Validate() == ConditionState.NotFulfilled));
        private bool CanTurnOffLamp() => !(AutomationDisabled || (_turnOffConditionsValidator.Validate() == ConditionState.NotFulfilled));
        
        //internal IEnumerable<MotionPoint> GetLastMovments(DateTimeOffset referenceTime) => _MotionHistory.GetLastElements(referenceTime, MotionDetectorAlarmTime).Select(time => new MotionPoint(MotionDetectorUid, time));

        internal void DisableAutomation() => AutomationDisabled = true;
        internal void EnableAutomation() => AutomationDisabled = false;
        internal void DisableAutomation(TimeSpan time)
        {
            DisableAutomation();
            _AutomationEnableOn = _scheduler.Now + time;
        }

    }
}