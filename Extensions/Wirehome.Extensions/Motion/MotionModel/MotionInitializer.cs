namespace Wirehome.Extensions.MotionModel
{
    public class MotionConfigurationProvider : IMotionConfigurationProvider
    {
        public int MotionTimeWindow { get; set; } = 3000;
        public int CollisionResolutionTime { get; set; } = 10000;
        public int MotionMinDiff { get; set; } = 500;  //minimal difference in movement that is possible to do physically
    }
}


