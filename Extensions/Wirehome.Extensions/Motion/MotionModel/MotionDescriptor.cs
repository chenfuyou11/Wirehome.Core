using System;
using System.Reactive.Concurrency;
using System.Collections.Generic;
using Wirehome.Conditions;
using Wirehome.Conditions.Specialized;
using Wirehome.Contracts.Conditions;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Actuators;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Core;
using Wirehome.Extensions.Motion;

namespace Wirehome.Extensions.MotionModel
{
    //TODO add change source in event to distinct the source of the change (manual light on or automatic)
    public class MotionDescriptor
    {
        private readonly ConditionsValidator _turnOnConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);
        private readonly ConditionsValidator _turnOffConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);
        private readonly object _syncRoot = new object();

        internal IObservable<PowerStateValue> PowerChangeSource { get; } // TODO Add descriptor for some codes for change on/off

        // Configuration parameters
        public string MotionDetectorUid { get; }
        public IEnumerable<string> Neighbors { get; }
        public ILamp Lamp { get; }
        public float LightIntensityAtNight { get; }
        public int MaxPersonCapacity { get; }
        public AreaType AreaType { get; }
        public int MotionDetectorAlarmTime { get; }

        // Dynamic parameters
        public TimeSpan TimeToLive { get; private set; }
        public double PresenceProbability { get; private set; }
        public bool AutomationDisabled { get; }
        public DateTimeOffset LastMotionTime { get; private set; }
        public MotionVector LastEnter { get; }
        public MotionVector LastLeave { get; }
        public TimeList MotionHistory { get; } //TODO Clear after some period
        private DateTime AutomationEnableOn { get; set; }
        public DateTimeOffset LastManualTurnOn { get; private set; }
        public int NumberOfPersonsInArea { get; private set; }
        private int _presenseMotionCounter { get; set; }

        // TODO
        // Add pending turnoff source - we could turn off but it was not sure last time

        public MotionDescriptor(string motionDetectorUid, IEnumerable<string> neighbors, ILamp lamp, IScheduler scheduler,
                                IDaylightService daylightService, IDateTimeService dateTimeService, AreaDescriptor initializer = null)
        {
            MotionDetectorUid = motionDetectorUid ?? throw new ArgumentNullException(nameof(motionDetectorUid));
            Neighbors = neighbors ?? throw new ArgumentNullException(nameof(neighbors));
            Lamp = lamp ?? throw new ArgumentNullException(nameof(lamp));
            MaxPersonCapacity = initializer?.MaxPersonCapacity ?? 10;
            AreaType = initializer?.AreaType ?? AreaType.Room;

            if (initializer?.DisableAtNight ?? true)
            {
                _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsDayCondition(daylightService, dateTimeService));
            }
            if (initializer?.DisableAtDayLight ?? true)
            {
                _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsNightCondition(daylightService, dateTimeService));
            }

            MotionHistory = new TimeList(scheduler);
            PowerChangeSource = Lamp.ToPowerChangeSource();
        }

        public void MarkMotion(DateTimeOffset time)
        {
            LastMotionTime = time;
            MotionHistory.Add(time);
            _presenseMotionCounter++;
            SetProbability(1.0);
        }

        public void SetProbability(double probability)
        {
            PresenceProbability = probability;

            //TODO theadsafe
            if(PresenceProbability == 1.0)
            {
                TryTurnOnLamp();
            }
            else if(PresenceProbability == 0)
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
            if (CanTurnOffLamp()) Lamp.ExecuteCommand(new TurnOffCommand());
        }

        private bool CanTurnOnLamp()
        {
            if (_turnOnConditionsValidator.Conditions.Count > 0 && _turnOnConditionsValidator.Validate() == ConditionState.Fulfilled)
            {
                return false;
            }

            return true;
        }

        private bool CanTurnOffLamp()
        {
            if (_turnOffConditionsValidator.Conditions.Count > 0 && _turnOffConditionsValidator.Validate() == ConditionState.Fulfilled)
            {
                return false;
            }

            return true;
        }
    }
}