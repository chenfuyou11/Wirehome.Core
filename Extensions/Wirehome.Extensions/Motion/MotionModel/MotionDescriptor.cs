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
using Wirehome.Extensions.Motion;
using Wirehome.Contracts.Components;
using System.Collections.ObjectModel;
using Wirehome.Extensions.Extensions;

namespace Wirehome.Extensions.MotionModel
{
    //TODO add change source in event to distinct the source of the change (manual light on or automatic)
    public class MotionDescriptor
    {
        private readonly ConditionsValidator _turnOnConditionsValidator = new ConditionsValidator();
        private readonly ConditionsValidator _turnOffConditionsValidator = new ConditionsValidator();
        private readonly object _syncRoot = new object();

        internal IObservable<PowerStateValue> PowerChangeSource { get; } // TODO Add descriptor for some codes for change on/off

        // Configuration parameters
        public string MotionDetectorUid { get; }
        internal IEnumerable<string> Neighbors { get; }
        internal IReadOnlyCollection<MotionDescriptor> NeighborsCache { get; private set; }
        private IComponent Lamp { get; }
        private float LightIntensityAtNight { get; }
        private int MaxPersonCapacity { get; }
        private AreaType AreaType { get; }
        private TimeSpan MotionDetectorAlarmTime { get; }

        // Dynamic parameters
        public bool AutomationDisabled { get; private set; }
        private TimeList _MotionHistory { get; }
        private TimeSpan _TimeToLive { get; set; }
        private double _PresenceProbability { get; set; }
        private DateTime _AutomationEnableOn { get; set; }
        private DateTimeOffset _LastManualTurnOn { get; set; }
        private int _NumberOfPersonsInArea { get; set; }
        private int _PresenseMotionCounter { get; set; }
        private DateTimeOffset _LastMotionTime { get; set; }
        private MotionVector _LastEnter { get; set; }
        private MotionVector _LastLeave { get; set; }


        // TODO
        // Add pending turnoff source - we could turn off but it was not sure last time

        public MotionDescriptor(string motionDetectorUid, IEnumerable<string> neighbors, IComponent lamp, IScheduler scheduler,
                                IDaylightService daylightService, IDateTimeService dateTimeService, AreaDescriptor areaDescriptor = null)
        {
            MotionDetectorUid = motionDetectorUid ?? throw new ArgumentNullException(nameof(motionDetectorUid));
            Neighbors = neighbors ?? throw new ArgumentNullException(nameof(neighbors));
            Lamp = lamp ?? throw new ArgumentNullException(nameof(lamp));
            MaxPersonCapacity = areaDescriptor?.MaxPersonCapacity ?? 10;
            AreaType = areaDescriptor?.AreaType ?? AreaType.Room;
            MotionDetectorAlarmTime = areaDescriptor?.MotionDetectorAlarmTime ?? TimeSpan.FromMilliseconds(2500);

            var workingTime = areaDescriptor?.WorkingTime ?? WorkingTime.AllDay;

            if (workingTime == WorkingTime.DayLight)
            {
                _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsDayCondition(daylightService, dateTimeService));
            }
            else if (workingTime == WorkingTime.AfterDusk)
            {
                _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsNightCondition(daylightService, dateTimeService));
            }

            _MotionHistory = new TimeList(scheduler);
            PowerChangeSource = Lamp.ToPowerChangeSource();
        }

        internal void BuildNeighborsCache(IEnumerable<MotionDescriptor> neighbors)
        {
            NeighborsCache = new ReadOnlyCollection<MotionDescriptor>(neighbors.ToList());
        }

        public void MarkMotion(DateTimeOffset time)
        {
            _LastMotionTime = time;
            _MotionHistory.Add(time);
            _PresenseMotionCounter++;
            SetProbability(1.0);

            NeighborsCache.ForEach(neighbor => neighbor.ResolveCofusion(new MotionPoint(MotionDetectorUid, time)));
        }

        private void ResolveCofusion(MotionPoint motionPoint)
        {
            
        }

        public void MarkEnter(MotionVector vector)
        {
            _LastEnter = vector;
            _NumberOfPersonsInArea++;
        }

        public void MarkLeave(MotionVector vector)
        {
            _LastLeave = vector;
            _NumberOfPersonsInArea--;

            if (vector.Confiusions.Count == 0 && MaxPersonCapacity == 1)
            {
                SetProbability(0);
            }
        }

        private void ResetStatistics()
        {
            _NumberOfPersonsInArea = 0;
            _MotionHistory.ClearOldData(MotionDetectorAlarmTime);
        }

        private void SetProbability(double probability)
        {
            _PresenceProbability = probability;

            //TODO theadsafe
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
        

        internal IEnumerable<MotionPoint> GetLastMovments(DateTimeOffset referenceTime) => _MotionHistory.GetLastElements(referenceTime, MotionDetectorAlarmTime).Select(time => new MotionPoint(MotionDetectorUid, time));

        internal void DisableAutomation() => AutomationDisabled = true;
        internal void EnableAutomation() => AutomationDisabled = false;
    }
}