using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Wirehome.Conditions;
using Wirehome.Conditions.Specialized;
using Wirehome.Contracts.Conditions;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Components;
using Wirehome.Motion.Model;
using Wirehome.Extensions;
using Wirehome.Extensions.Extensions;
using System.Reactive;
using Wirehome.Extensions.Core;

namespace Wirehome.Motion
{
    //TODO Thread safe
    //TODO add change source in event to distinct the source of the change (manual light on or automatic)
    //TODO Manual turn off/on - react if was just after auto, code some automatic reaction (code from manual turn on/off)
    public class Room : IDisposable
    {
        private readonly ConditionsValidator _turnOnConditionsValidator = new ConditionsValidator();
        private readonly ConditionsValidator _turnOffConditionsValidator = new ConditionsValidator();
        private readonly IScheduler _scheduler;
        private readonly MotionConfiguration _motionConfiguration;
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();

        // Configuration parameters
        public string Uid { get; }
        internal IEnumerable<string> Neighbors { get; }
        internal IReadOnlyCollection<Room> NeighborsCache { get; private set; }
    
        // Dynamic parameters
        internal bool AutomationDisabled { get; private set; }
        internal int NumberOfPersonsInArea { get; private set; }
        internal MotionStamp LastMotion { get; } = new MotionStamp();
        internal AreaDescriptor AreaDescriptor { get; }
        internal MotionVector LastVectorEnter { get; private set; }

        private IMotionLamp Lamp { get; }
        private TimeList<DateTimeOffset> _MotionHistory { get; }
        private Probability _PresenceProbability { get; set; } = Probability.Zero;
        private DateTimeOffset _AutomationEnableOn { get; set; }
        private int _PresenseMotionCounter { get; set; }
        private DateTimeOffset? _LastAutoIncrement;
        private readonly IConcurrencyProvider _concurrencyProvider;
        private readonly IEnumerable<IEventDecoder> _eventsDecoders;

        private float LightIntensityAtNight { get; }
        private DateTimeOffset? _LastAutoTurnOff { get; set; }

        public override string ToString()
        {
            //return $"{MotionDetectorUid} [Last move: {LastMotionTime}] [Persons: {NumberOfPersonsInArea}]";
            //TODO DEBUG
            return $"{Uid} [Last move: {LastMotion}] [Persons: {NumberOfPersonsInArea}] [Lamp: {(Lamp as MotionLamp)?.IsTurnedOn}]";
        }

        public Room(string uid, IEnumerable<string> neighbors, IMotionLamp lamp, IScheduler scheduler, IDaylightService daylightService,
                    IDateTimeService dateTimeService, IConcurrencyProvider concurrencyProvider, AreaDescriptor areaDescriptor, MotionConfiguration motionConfiguration, 
                    IEnumerable<IEventDecoder> eventsDecoders)
        {
            Uid = uid ?? throw new ArgumentNullException(nameof(uid));
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

            _MotionHistory = new TimeList<DateTimeOffset>(scheduler);

            _scheduler = scheduler;
            _motionConfiguration = motionConfiguration;
            _concurrencyProvider = concurrencyProvider;
            _eventsDecoders = eventsDecoders;
            AreaDescriptor = areaDescriptor;
            
            _eventsDecoders?.ForEach(decoder => decoder.Init(this));
        }
        
        internal void RegisterLampManualChangeEvents()
        {
            if (Lamp.PowerStateChange == null) return;

            var manualEventSource = Lamp.PowerStateChange
                                        .Where(ev => ev.EventSource == PowerStateChangeEvent.ManualSource);
            
            var subscription = manualEventSource.Timestamp()
                                                .Buffer(manualEventSource, _ => Observable.Timer(_motionConfiguration.ManualCodeWindow, _concurrencyProvider.Scheduler))
                                                .Subscribe(DecodeMessage);

            _disposeContainer.Add(subscription);
        }

        private void DecodeMessage(IList<Timestamped<PowerStateChangeEvent>> powerStateEvents) => _eventsDecoders?.ForEach(decoder => decoder.DecodeMessage(powerStateEvents));
        

        public void MarkMotion(DateTimeOffset time)
        {
            if (_PresenceProbability == Probability.Zero && time.HappendInPrecedingTimeWindow(_LastAutoTurnOff, _motionConfiguration.MotionTimeWindow))
            {
                AreaDescriptor.TurnOffTimeout = AreaDescriptor.TurnOffTimeout.IncreaseByPercentage(_motionConfiguration.TurnOffTimeoutIncrementPercentage);
            }

            LastMotion.SetTime(time);
            _MotionHistory.Add(time);
            _PresenseMotionCounter++;
            SetProbability(Probability.Full);

            AutoIncrementForOnePerson(time);
        }

        public void Update()
        {
            CheckForTurnOnAutomationAgain();
            RecalculateProbability();
        }

        public void MarkEnter(MotionVector vector)
        {
            LastVectorEnter = vector;
            IncrementNumberOfPersons(vector.End.TimeStamp);
        }

        public void MarkLeave(MotionVector vector)
        {
            DecrementNumberOfPersons();

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

        internal void BuildNeighborsCache(IEnumerable<Room> neighbors) => NeighborsCache = new ReadOnlyCollection<Room>(neighbors.ToList());
        internal void DisableAutomation() => AutomationDisabled = true;
        internal void EnableAutomation() => AutomationDisabled = false;

        internal void DisableAutomation(TimeSpan time)
        {
            DisableAutomation();
            _AutomationEnableOn = _scheduler.Now + time;
        }

        internal IList<MotionPoint> GetConfusingPoints(MotionVector vector) => NeighborsCache.ToList()
                                                                                             .AddChained(this)
                                                                                             .Where(room => room.Uid != vector.Start.Uid)
                                                                                             .Select(room => room.GetConfusion(vector.End.TimeStamp))
                                                                                             .Where(y => y != null)
                                                                                             .ToList();

        /// <summary>
        /// When we don't detect motion vector previously but there is move in room and currently we have 0 person so we know that there is a least one
        /// </summary>
        private void AutoIncrementForOnePerson(DateTimeOffset time)
        {
            if (NumberOfPersonsInArea == 0)
            {
                _LastAutoIncrement = time;
                NumberOfPersonsInArea++;
            }
        }

        private void IncrementNumberOfPersons(DateTimeOffset time)
        {
            if (!_LastAutoIncrement.HasValue || time.HappendBeforePrecedingTimeWindow(_LastAutoIncrement, TimeSpan.FromMilliseconds(100)))
            {
                NumberOfPersonsInArea++;
            }
        }

        private void DecrementNumberOfPersons()
        {
            if (NumberOfPersonsInArea > 0)
            {
                NumberOfPersonsInArea--;

                if(NumberOfPersonsInArea == 0)
                {
                    LastMotion.UnConfuze();
                }
            }
        }

        private void ZeroNumberOfPersons()
        {
            NumberOfPersonsInArea = 0;
        }

        private MotionPoint GetConfusion(DateTimeOffset timeOfMotion)
        {
            var lastMotion = LastMotion;

            // If last motion time has same value we have to go back in time for previous value to check real previous
            if (timeOfMotion == lastMotion.Time)
            {
                lastMotion = lastMotion.Previous;
            }

            if
            (
                  lastMotion?.Time != null
               && lastMotion.CanConfuze
               && timeOfMotion.IsMovePhisicallyPosible(lastMotion.Time.Value, _motionConfiguration.MotionMinDiff)
               && timeOfMotion.HappendInPrecedingTimeWindow(lastMotion.Time, AreaDescriptor.MotionDetectorAlarmTime)
            )
            {
                return new MotionPoint(Uid, lastMotion.Time.Value);
            }

            return null;
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
            ZeroNumberOfPersons();
            _MotionHistory.ClearOldData(AreaDescriptor.MotionDetectorAlarmTime);
            _LastAutoTurnOff = _scheduler.Now;
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

        public void Dispose()
        {
            _disposeContainer.Dispose();
        }
    }
}