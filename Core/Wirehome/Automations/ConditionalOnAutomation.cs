using System;
using Wirehome.Conditions;
using Wirehome.Conditions.Specialized;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Services;
using Wirehome.Triggers;

namespace Wirehome.Automations
{
    public class ConditionalOnAutomation : Automation
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IDaylightService _daylightService;

        public ConditionalOnAutomation(string id, ISchedulerService schedulerService, IDateTimeService dateTimeService, IDaylightService daylightService) 
            : base(id)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));

            _dateTimeService = dateTimeService;
            _daylightService = daylightService;
            
            WithTrigger(new IntervalTrigger(TimeSpan.FromMinutes(1), schedulerService));
        }

        public ConditionalOnAutomation WithOnAtNightRange()
        {
            var nightCondition = new TimeRangeCondition(_dateTimeService).WithStart(_daylightService.Sunset).WithEnd(_daylightService.Sunrise);
            WithCondition(ConditionRelation.And, nightCondition);

            return this;
        }

        public ConditionalOnAutomation WithOffBetweenRange(TimeSpan from, TimeSpan until)
        {
            WithCondition(ConditionRelation.And, new TimeRangeCondition(_dateTimeService).WithStart(() => from).WithEnd(() => until).WithInversion());

            return this;
        }

        public ConditionalOnAutomation WithComponent(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            WithActionIfConditionsFulfilled(() => component.ExecuteCommand(new TurnOnCommand()));
            WithActionIfConditionsNotFulfilled(() => component.ExecuteCommand(new TurnOffCommand()));

            return this;
        }
    }
}
