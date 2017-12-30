using System;

namespace Wirehome.Extensions.MotionModel
{
    public class MotionConfigurationProvider : IMotionConfigurationProvider
    {
        public TimeSpan MotionTimeWindow { get; set; } = TimeSpan.FromMilliseconds(3000);
        public TimeSpan CollisionResolutionTime { get; set; } = TimeSpan.FromMilliseconds(10000);
        public TimeSpan MotionMinDiff { get; set; } = TimeSpan.FromMilliseconds(500);  //minimal difference in movement that is possible to do physically
        public TimeSpan PeriodicCheckTime { get; set; } = TimeSpan.FromMilliseconds(1000);
    }
}


