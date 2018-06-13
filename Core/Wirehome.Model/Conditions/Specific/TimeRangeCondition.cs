using System;
using System.Threading.Tasks;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core;
using Wirehome.Model.Conditions;
using Wirehome.Model.Extensions;

namespace Wirehome.Conditions.Specialized
{
    public class TimeRangeCondition : Condition
    {
        private Func<Task<TimeSpan>> _startValueProvider;
        private Func<Task<TimeSpan>> _endValueProvider;


        public TimeRangeCondition WithStart(Func<Task<TimeSpan>> start)
        {
            _startValueProvider = start;
            return this;
        }

        public TimeRangeCondition WithEnd(Func<Task<TimeSpan>> end)
        {
            _endValueProvider = end;
            return this;
        }

        public TimeRangeCondition WithStart(TimeSpan start)
        {
            this[ConditionProperies.StartTime] = new TimeSpanValue(start);
            return this;
        }

        public TimeRangeCondition WithEnd(TimeSpan end)
        {
            this[ConditionProperies.EndTime] = new TimeSpanValue(end);
            return this;
        }

        public TimeRangeCondition WithStartAdjustment(TimeSpan value)
        {
            this[ConditionProperies.StartAdjustment] = new TimeSpanValue(value);
            return this;
        }

        public TimeRangeCondition WithEndAdjustment(TimeSpan value)
        {
            this[ConditionProperies.EndAdjustment] = new TimeSpanValue(value);
            return this;
        }

        public override async Task<bool> Validate()
        {
            TimeSpan startValue = GetPropertyValue(ConditionProperies.StartTime, (TimeSpanValue)(await _startValueProvider().ConfigureAwait(false))).ToTimeSpanValue();
            TimeSpan endValue = GetPropertyValue(ConditionProperies.EndTime, (TimeSpanValue)(await _endValueProvider().ConfigureAwait(false))).ToTimeSpanValue();

            startValue += GetPropertyValue(ConditionProperies.StartAdjustment, (TimeSpanValue)TimeSpan.Zero).ToTimeSpanValue();
            endValue += GetPropertyValue(ConditionProperies.EndAdjustment, (TimeSpanValue)TimeSpan.Zero).ToTimeSpanValue();

            return SystemTime.Now.TimeOfDay.IsTimeInRange(startValue, endValue);
        }
    }
}
