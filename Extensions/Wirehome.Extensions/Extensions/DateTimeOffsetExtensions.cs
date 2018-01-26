using System;
using System.Collections.Generic;
using System.Text;

namespace Wirehome.Extensions.Extensions
{
   public static class DateTimeOffsetExtensions
   {
        public static bool HappendBefore(this DateTimeOffset time, DateTimeOffset? comparedTime, TimeSpan timeToCheck) => comparedTime < time && time - comparedTime < timeToCheck;
   }
}
