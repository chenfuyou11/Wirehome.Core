using System;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Sensors;

namespace Wirehome.Sensors.MotionDetectors
{
    public static class MotionDetectorExtensions
    {
        public static IMotionDetector GetMotionDetector(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IMotionDetector>($"{area.Id}.{id}");
        }
    }
}
