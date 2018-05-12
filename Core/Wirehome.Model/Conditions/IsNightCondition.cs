using System;
using Wirehome.Contracts.Environment;

namespace Wirehome.Conditions.Specialized
{
    public class IsNightCondition : TimeRangeCondition
    {
        public IsNightCondition(IDaylightService daylightService)
        {
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));

            WithStart(() => daylightService.Sunset);
            WithEnd(() => daylightService.Sunrise);
        }
    }
}
