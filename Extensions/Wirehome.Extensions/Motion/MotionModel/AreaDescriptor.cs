namespace Wirehome.Extensions.MotionModel
{
    public class AreaDescriptor
    {
        public bool? DisableAtNight { get; set; }
        public bool? DisableAtDayLight { get; set; }
        public int? MaxPersonCapacity { get; set; }
        public AreaType? AreaType { get; set; }
    }
}


