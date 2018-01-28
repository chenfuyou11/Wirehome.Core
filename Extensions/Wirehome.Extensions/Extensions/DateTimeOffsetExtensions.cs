using System;
using System.Collections.Generic;
using System.Text;

namespace Wirehome.Extensions.Extensions
{
   public static class DateTimeOffsetExtensions
   {
        public static bool HappendInPrecedingTimeWindow(this DateTimeOffset time, DateTimeOffset? comparedTime, TimeSpan timeWindow) => comparedTime < time && time - comparedTime < timeWindow;

        public static bool HappendBeforePrecedingTimeWindow(this DateTimeOffset time, DateTimeOffset? comparedTime, TimeSpan timeWindow) => comparedTime < time && time - comparedTime > timeWindow;

        public static bool IsMovePhisicallyPosible(this DateTimeOffset time, DateTimeOffset comparedTime, TimeSpan motionMinDiff) => TimeSpan.FromTicks(Math.Abs(time.Ticks - comparedTime.Ticks)) >= motionMinDiff;
    }
}
