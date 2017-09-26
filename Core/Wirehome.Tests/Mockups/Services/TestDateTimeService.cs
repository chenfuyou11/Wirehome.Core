using System;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Services;

namespace Wirehome.Tests.Mockups.Services
{
    public class TestDateTimeService : ServiceBase, IDateTimeService 
    {
        public DateTime DateTime { get; set; }

        public DateTime Date => DateTime.Date;
    
        public TimeSpan Time => DateTime.TimeOfDay;
    
        public DateTime Now => DateTime;

        public void SetTime(TimeSpan time)
        {
            DateTime = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, time.Hours, time.Minutes, time.Seconds);
        }
    }
}
