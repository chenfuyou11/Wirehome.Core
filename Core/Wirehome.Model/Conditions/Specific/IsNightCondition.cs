﻿using Wirehome.ComponentModel.Commands;
using Wirehome.Core.EventAggregator;
using Wirehome.Model.Conditions;
using Wirehome.Model.Extensions;

namespace Wirehome.Conditions.Specialized
{
    public class IsNightCondition : TimeRangeCondition
    {
        public IsNightCondition(IEventAggregator eventAggregator)
        {
            WithStart(async () => (await eventAggregator.QueryForValueType(CommandFatory.GetSunsetCommand, ConditionProperies.EndTime).ConfigureAwait(false)).ToTimeSpanValue());
            WithEnd(async () => (await eventAggregator.QueryForValueType(CommandFatory.GetSunriseCommand, ConditionProperies.StartTime).ConfigureAwait(false)).ToTimeSpanValue());
        }
    }
}