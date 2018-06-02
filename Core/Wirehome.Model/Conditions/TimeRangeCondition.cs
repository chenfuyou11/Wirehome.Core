using System;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Contracts.Conditions;
using Wirehome.Core;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Model.Conditions;
using Wirehome.Model.Extensions;

namespace Wirehome.Conditions.Specialized
{
    public class TimeRangeCondition : Condition
    {
        private Func<TimeSpan> _startValueProvider;
        private Func<TimeSpan> _endValueProvider;

        public TimeRangeCondition()
        {
            WithExpression(() => Check());
        }


        public TimeRangeCondition WithStart(Func<TimeSpan> start)
        {
            _startValueProvider = start ?? throw new ArgumentNullException(nameof(start));
            return this;
        }

        public TimeRangeCondition WithEnd(Func<TimeSpan> end)
        {
            _endValueProvider = end ?? throw new ArgumentNullException(nameof(end));
            return this;
        }

        public TimeRangeCondition WithStart(TimeSpan start)
        {
            this[ConditionProperiesConstants.StartTime] = new TimeSpanValue(start);
            return this;
        }

        public TimeRangeCondition WithEnd(TimeSpan end)
        {
            this[ConditionProperiesConstants.EndTime] = new TimeSpanValue(end);
            return this;
        }

        public TimeRangeCondition WithStartAdjustment(TimeSpan value)
        {
            this[ConditionProperiesConstants.StartAdjustment] = new TimeSpanValue(value);
            return this;
        }

        public TimeRangeCondition WithEndAdjustment(TimeSpan value)
        {
            this[ConditionProperiesConstants.EndAdjustment] = new TimeSpanValue(value);
            return this;
        }

        private ConditionState Check()
        {
            if (_startValueProvider == null || _endValueProvider == null)
            {
                return ConditionState.NotFulfilled;
            }

            TimeSpan startValue = GetPropertyValue(ConditionProperiesConstants.StartTime, (TimeSpanValue)_startValueProvider()).ToTimeSpanValue();
            TimeSpan endValue = GetPropertyValue(ConditionProperiesConstants.EndTime, (TimeSpanValue)_endValueProvider()).ToTimeSpanValue();

            startValue += GetPropertyValue(ConditionProperiesConstants.StartAdjustment, (TimeSpanValue)TimeSpan.Zero).ToTimeSpanValue();
            endValue += GetPropertyValue(ConditionProperiesConstants.EndAdjustment, (TimeSpanValue)TimeSpan.Zero).ToTimeSpanValue();

            //TODO check
            if (SystemTime.Now.TimeOfDay.IsTimeInRange(startValue, endValue))
            {
                return ConditionState.Fulfilled;
            }
            
            return ConditionState.NotFulfilled;
        }
    }
}
