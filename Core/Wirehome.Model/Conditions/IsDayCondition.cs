using System;
using Wirehome.Contracts.Environment;


namespace Wirehome.Conditions.Specialized
{
    public class IsDayCondition : TimeRangeCondition
    {
        public IsDayCondition(IDaylightService daylightService)
        {
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));

            WithStart(daylightService.Sunrise);
            WithEnd(daylightService.Sunset);
        }
    }
}
