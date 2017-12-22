using System;

namespace Wirehome.Extensions.MotionModel
{
    public class AreaDescriptor
    {
        public WorkingTime WorkingTime { get; set; }                
        public int? MaxPersonCapacity { get; set; }                 // How many persons can be at once in single rom
        public AreaType? AreaType { get; set; }
        public TimeSpan? MotionDetectorAlarmTime { get; set; }      // How long it takes alarm to be able to detect move
        public float? LightIntensityAtNight { get; set; }           // When using dimmers we would like to make intenity diffrent at night
    }
}


