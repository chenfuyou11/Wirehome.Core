using System;

namespace Wirehome.Extensions.MotionModel
{
    public interface IMotionConfigurationProvider
    {
        TimeSpan CollisionResolutionTime { get; set; }
        TimeSpan MotionMinDiff { get; set; }
        TimeSpan MotionTimeWindow { get; set; }
        TimeSpan PeriodicCheckTime { get; set; }
    }
}