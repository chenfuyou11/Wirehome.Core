namespace Wirehome.Extensions.MotionModel
{
    public interface IMotionConfigurationProvider
    {
        int CollisionResolutionTime { get; set; }
        int MotionMinDiff { get; set; }
        int MotionTimeWindow { get; set; }
    }
}