using System;
using System.Collections.Generic;
using System.Text;

namespace Wirehome.Core
{
    public static class SystemTime
    {
        internal static Func<DateTimeOffset> SetCurrentTime = () => DateTimeOffset.Now;

        public static DateTimeOffset Now
        {
            get
            {
                return SetCurrentTime();
            }
        }
    }

}
