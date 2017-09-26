using System;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Environment;

namespace Wirehome.Extensions.Tests
{
    public class TestDaylightService : ServiceBase, IDaylightService
    {
        public TestDaylightService()
        {
            Sunrise = TimeSpan.Parse("06:00");
            Sunset = TimeSpan.Parse("18:00");
        }

        public TimeSpan Sunrise { get; set; }
        public TimeSpan Sunset { get; set; }
        public DateTime? Timestamp { get; set; }
        public void Update(TimeSpan sunrise, TimeSpan sunset)
        {
        }
    }
}
