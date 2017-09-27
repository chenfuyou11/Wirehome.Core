using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Wirehome.Automations;
using System;

namespace Wirehome.Extensions.Tests
{
    [TestClass]
    public class SchedulerTest
    {
        [TestMethod]
        public void TestSchedule()
        {
            var daySchedule = new DaySchedule()
            {
                Cyclic = DayCyclic.All,
                DayActivitySchedule = new List<ActivityDescriptor>
                {
                    new ActivityDescriptor{ Start = new TimeSpan(8, 0, 0), End = new TimeSpan(10, 0, 0), Value = 1},
                    new ActivityDescriptor{ Start = new TimeSpan(12, 00, 00), End = new TimeSpan(14, 0, 0), Value = 2}
                }
            };
            var dateTimeService = new TestDateTimeService();
            var pomp = new PompScheduler(110, 2000, new TimeSpan(0, 1, 0), dateTimeService, daySchedule);

            var schedule = pomp.CalculateDaySchedule();

            Assert.AreEqual(1, 1);
        }
    }
}
