using System;

namespace Wirehome.Core
{
    public static class SystemTime
    {
        internal static Func<DateTimeOffset> SetCurrentTime = () => DateTimeOffset.Now;
        public static DateTimeOffset Now => SetCurrentTime();
    }
}
