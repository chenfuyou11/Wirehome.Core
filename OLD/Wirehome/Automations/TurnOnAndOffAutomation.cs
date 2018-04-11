using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Wirehome.Components;
using Wirehome.Conditions;
using Wirehome.Conditions.Specialized;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Conditions;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Sensors;
using Wirehome.Contracts.Sensors.Events;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Settings;
using Wirehome.Contracts.Triggers;
using Wirehome.Scheduling;
using Wirehome.Triggers;
using System.Globalization;

namespace Wirehome.Automations
{
    public class TurnOnAndOffAutomation : AutomationBase
    {
        private readonly IMessageBrokerService _messageBroker;
        private readonly ISettingsService _settingsService;
        private readonly object _syncRoot = new object();
        private readonly List<IComponent> _components = new List<IComponent>();

        private readonly ConditionsValidator _enablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);
        private readonly ConditionsValidator _disablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);
        private readonly ConditionsValidator _turnOffConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);

        private readonly IDateTimeService _dateTimeService;
        private readonly ISchedulerService _schedulerService;
        private readonly IDaylightService _daylightService;

        private readonly Stopwatch _lastTurnedOn = new Stopwatch();

        private TimeSpan? _pauseDuration;
        private IScheduledAction _turnOffTimeout;
        private bool _turnOffIfButtonPressedWhileAlreadyOn;
        private bool _isOn;
  
        public TurnOnAndOffAutomation(
            string id,
            IDateTimeService dateTimeService,
            ISchedulerService schedulerService,
            ISettingsService settingsService,
            IDaylightService daylightService,
            IMessageBrokerService messageBroker)
            : base(id)
        {
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _daylightService = daylightService ?? throw new ArgumentNullException(nameof(daylightService));

            settingsService.CreateSettingsMonitor<TurnOnAndOffAutomationSettings>(this, s => Settings = s.NewSettings);
        }

        public TurnOnAndOffAutomationSettings Settings { get; private set; }

        public TurnOnAndOffAutomation WithTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            _messageBroker.CreateTrigger<MotionDetectedEvent>(motionDetector.Id).Attach(ExecuteAutoTrigger);
            _messageBroker.CreateTrigger<MotionDetectionCompletedEvent>(motionDetector.Id).Attach(StartTimeout);

            _settingsService.CreateSettingsMonitor<MotionDetectorSettings>(motionDetector, CancelTimeoutIfMotionDetectorDeactivated);

            return this;
        }

        public TurnOnAndOffAutomation WithTrigger(ITrigger trigger)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            trigger.Attach(ExecuteManualTrigger);
            return this;
        }

        public TurnOnAndOffAutomation WithTarget(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            _components.Add(component);
            return this;
        }

        public TurnOnAndOffAutomation WithTurnOnWithinTimeRange(Func<TimeSpan> from, Func<TimeSpan> until)
        {
            if (@from == null) throw new ArgumentNullException(nameof(from));
            if (until == null) throw new ArgumentNullException(nameof(until));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_dateTimeService).WithStart(from).WithEnd(until));
            return this;
        }

        public TurnOnAndOffAutomation WithEnablingCondition(ConditionRelation relation, ICondition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            _enablingConditionsValidator.WithCondition(relation, condition);
            return this;
        }

        public TurnOnAndOffAutomation WithEnabledAtDay()
        {
            TimeSpan Start() => _daylightService.Sunrise.Add(TimeSpan.FromHours(1));
            TimeSpan End() => _daylightService.Sunset.Subtract(TimeSpan.FromHours(1));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_dateTimeService).WithStart(Start).WithEnd(End));
            return this;
        }

        public TurnOnAndOffAutomation WithEnabledAtNight(TimeSpan? sunriseShift = null, TimeSpan? sunsetShift = null)
        {
            TimeSpan Start() => _daylightService.Sunset.Add(sunsetShift.GetValueOrDefault(TimeSpan.FromHours(-1)));
            TimeSpan End() => _daylightService.Sunrise.Add(sunriseShift.GetValueOrDefault(TimeSpan.FromHours(1)));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_dateTimeService).WithStart(Start).WithEnd(End));
            return this;
        }

        public TurnOnAndOffAutomation WithSkipIfAnyIsAlreadyOn(params IComponent[] components)
        {
            if (components == null) throw new ArgumentNullException(nameof(components));

            _disablingConditionsValidator.WithCondition(ConditionRelation.Or,
                new Condition().WithExpression(() => components.Any(a => a.GetState().Has(PowerState.On))));

            return this;
        }

        public TurnOnAndOffAutomation WithTurnOffIfButtonPressedWhileAlreadyOn()
        {
            _turnOffIfButtonPressedWhileAlreadyOn = true;
            return this;
        }

        public TurnOnAndOffAutomation WithPauseAfterEveryTurnOn(TimeSpan duration)
        {
            _pauseDuration = duration;
            return this;
        }

        private void CancelTimeoutIfMotionDetectorDeactivated(SettingsChangedEventArgs<MotionDetectorSettings> e)
        {
            if (!e.NewSettings.IsEnabled)
            {
                lock (_syncRoot)
                {
                    _turnOffTimeout?.Cancel();
                }
            }
        }

        private void ExecuteManualTrigger()
        {
            lock (_syncRoot)
            {
                if (_isOn)
                {
                    if (_turnOffIfButtonPressedWhileAlreadyOn)
                    {
                        TurnOff();
                        return;
                    }

                    StartTimeout();
                }
                else
                {
                    TurnOn();
                    StartTimeout();
                }
            }
        }

        private void ExecuteAutoTrigger()
        {
            lock (_syncRoot)
            {
                if (!Settings.IsEnabled)
                {
                    return;
                }

                if (!GetConditionsAreFulfilled())
                {
                    return;
                }

                if (IsPausing())
                {
                    return;
                }

                TurnOn();
            }
        }

        private void StartTimeout()
        {
            lock (_syncRoot)
            {
                if (!GetConditionsAreFulfilled())
                {
                    return;
                }

                _turnOffTimeout?.Cancel();
                _turnOffTimeout = ScheduledAction.Schedule(Settings.Duration, TurnOff);
            }
        }

        private bool IsPausing()
        {
            if (!_pauseDuration.HasValue)
            {
                return false;
            }

            if (_lastTurnedOn.Elapsed < _pauseDuration.Value)
            {
                return true;
            }

            return false;
        }

        private void TurnOn()
        {
            _turnOffTimeout?.Cancel();

            _components.ForEach(c => c.TryTurnOn());

            _isOn = true;
            _lastTurnedOn.Restart();
        }

        private void TurnOff()
        {
            _turnOffTimeout?.Cancel();
            if (GetTurnOffConditionsAreFulfilled())
            {

                _components.ForEach(c => c.TryTurnOff());

                _isOn = false;
                _lastTurnedOn.Stop();
            }
        }



        private bool GetConditionsAreFulfilled()
        {
            if (_disablingConditionsValidator.Conditions.Any() && _disablingConditionsValidator.Validate() == ConditionState.Fulfilled)
            {
                return false;
            }

            if (_enablingConditionsValidator.Conditions.Any() && _enablingConditionsValidator.Validate() == ConditionState.NotFulfilled)
            {
                return false;
            }

            return true;
        }

        public TurnOnAndOffAutomation WithTurnOffCondition(ConditionRelation relation, ICondition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            _turnOffConditionsValidator.WithCondition(relation, condition);
            return this;
        }

        public TurnOnAndOffAutomation WithDisableTurnOffWhenBinaryStateEnabled(IBinaryInput input)
        {
            return WithTurnOffCondition(ConditionRelation.Or, new BinaryInputStateCondition(input, BinaryState.High));
        }

        private bool GetTurnOffConditionsAreFulfilled()
        {
            if (_turnOffConditionsValidator.Conditions.Any() && _turnOffConditionsValidator.Validate() == ConditionState.Fulfilled)
            {
                return false;
            }

            return true;
        }

        public TurnOnAndOffAutomation WithSchedulerTime(AutomationScheduler schedulerConfig)
        {
            _schedulerService.Register(Guid.NewGuid().ToString(), schedulerConfig.GetNextTurnOnTime(), () =>
            {
                ExecuteAutoTrigger();

                if (schedulerConfig.ScheduleTurnOff)
                {
                    Settings.Duration = schedulerConfig.GetNextTurnOffTime();

                    StartTimeout();
                }
            }, true);

            return this;
        }
    }

    public class AutomationScheduler
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan TurnOnInterval { get; set; }
        public TimeSpan WorkingTime { get; set; }

        private bool _firstTimeStart = true;

        private readonly IDateTimeService _dateTimeService;

        public AutomationScheduler(IDateTimeService dateTimeService)
        {
            _dateTimeService = dateTimeService;
        }

        public AutomationScheduler()
        {
            
        }

        public TimeSpan GetNextTurnOnTime()
        {
            if (_firstTimeStart)
            {
                _firstTimeStart = false;

                var now = _dateTimeService.Time;
                var scheduled = StartTime;
                TimeSpan timeToStartScheduler;


                if (scheduled > now)
                {
                    var diff = (int)((scheduled - now).TotalSeconds / TurnOnInterval.TotalSeconds);
                    timeToStartScheduler = scheduled - (TimeSpan.FromSeconds(diff * TurnOnInterval.TotalSeconds) + now);
                }
                else
                {
                    var diff = (int)((now - scheduled).TotalSeconds / TurnOnInterval.TotalSeconds) + 1;
                    timeToStartScheduler = ((TimeSpan.FromSeconds(diff * TurnOnInterval.TotalSeconds) + scheduled)) - now;
                }

                return timeToStartScheduler;
            }
            else
            {
                return TurnOnInterval;
            }
        }

        public bool ScheduleTurnOff
        {
            get
            {
                return WorkingTime.Ticks > 0;
            }

        }

        public TimeSpan GetNextTurnOffTime()
        {
            return WorkingTime;
        }


        public TimeSpan TotalWorkPerHour
        {
            get
            {
                return TimeSpan.FromSeconds(((int)(60 * 60 / (TurnOnInterval + WorkingTime).TotalSeconds)) * WorkingTime.TotalSeconds);
            }
        }

        public TimeSpan TotalWorkPerDay
        {
            get
            {
                return TimeSpan.FromSeconds(((int)(60 * 60 * 24 / (TurnOnInterval + WorkingTime).TotalSeconds)) * WorkingTime.TotalSeconds);
            }
        }
    }

    public interface IScheduleProvider
    {
        List<ScheduleActivity> CalculateDaySchedule();
    }
    
    public struct ScheduleActivity
    {
        public TimeSpan TurnOnTime { get; set; }
        public TimeSpan WorkingTime { get; set; }

        public override string ToString()
        {
            return $"{TurnOnTime} : {WorkingTime}";
        }
    }

    public class ActivityDescriptor
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public double Value { get; set; }
    }

    public struct DaySchedule
    {
        public List<ActivityDescriptor> DayActivitySchedule { get; set; }
        public DayCyclic Cyclic { get; set; }
    }

    [Flags]
    public enum DayCyclic
    {
        None = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64,
        WorkingDay = 128,
        Weekend = 256,
        All = 512
    }



    public class PompScheduler : IScheduleProvider
    {
        private const int HOURS_IN_DAY = 24;
        private int _waterPerMinuteCapacity;
        private int _dailyWaterConsumption;
        private TimeSpan _activityWorkingTime;
        private readonly IDateTimeService _dateTimeService;
        private DaySchedule _daySchedule;

        /// <summary>
        /// Generate scheduler for water pump
        /// </summary>
        /// <param name="waterPerMinuteConsumption">Water that is pumped thru the manadged system in one minute period - value in [mL]</param>
        /// <param name="dailyWaterConsumption">Desired water that should be pumped in whole day</param>
        /// <param name="pompMaxWorkingTime">Working time for single working segment</param>
        public PompScheduler(int waterPerMinuteConsumption, int dailyWaterConsumption, TimeSpan pompMaxWorkingTime, IDateTimeService dateTimeService, DaySchedule daySchedule)
        {
            _waterPerMinuteCapacity = waterPerMinuteConsumption;
            _dailyWaterConsumption = dailyWaterConsumption;
            _activityWorkingTime = pompMaxWorkingTime;
            _dateTimeService = dateTimeService;
            _daySchedule = daySchedule;

            ValidateInput();
        }

        private void ValidateInput()
        {
            if(_daySchedule.DayActivitySchedule.Any(x => x.Start > x.End))
            {
                throw new ArgumentException("Start of activity should not be set after end of it");
            }

            if(_daySchedule.DayActivitySchedule.Any(e => _daySchedule.DayActivitySchedule.Any(ev => e != ev && ev.Start < e.End && ev.End > e.Start)))
            {
                throw new ArgumentException("Activities should not overlap with each other");
            }
        }

        public List<ScheduleActivity> CalculateDaySchedule()
        {
            // Calculate day of work
            //_dateTimeService.Now;
            var activities = new List<ScheduleActivity>();
            var currentTime = new TimeSpan(0);
            var pomp_day_work_time = CalculatePompDailyWorkTime();

            // Time of work during the day
            var day_work_time = _daySchedule.DayActivitySchedule.Where(x => x.Value > 0).Sum(y => y.End.Ticks - y.Start.Ticks);
            // How much value we spent in whole day
            var day_work_value = _daySchedule.DayActivitySchedule.Sum(x => x.Value * (x.End.Ticks - x.Start.Ticks));

            foreach (var period in _daySchedule.DayActivitySchedule)
            {
                currentTime = period.Start;

                if (period.Value > 0)
                {
                    var period_time = period.End.Ticks - period.Start.Ticks;
                    // What percentage of whole day is in this period
                    var period_work_percentage = (period.Value * period_time) / day_work_value;

                    // Working time for this period
                    var period_working_time = period_work_percentage * pomp_day_work_time.Ticks;

                    var activitiesNumber = period_working_time / _activityWorkingTime.Ticks;
                    var activitiesNumberRounded = (int)Math.Ceiling((decimal)activitiesNumber);

                    var timeBetweenActivities = period_time / activitiesNumberRounded;

                    for (int i = 0; i < activitiesNumberRounded; i++)
                    {
                        activities.Add(new ScheduleActivity
                        {
                            TurnOnTime = currentTime,
                            WorkingTime = _activityWorkingTime.Ticks < period_working_time ? _activityWorkingTime : TimeSpan.FromTicks((long)period_working_time)
                        });

                        period_working_time -= _activityWorkingTime.Ticks;
                        currentTime = new TimeSpan(currentTime.Ticks + (long)timeBetweenActivities);
                    }
                }
            }

            return activities;
        }

        public TimeSpan CalculatePompDailyWorkTime()
        {
            return TimeSpan.FromMinutes(_dailyWaterConsumption / _waterPerMinuteCapacity);          
        }
    }


}


