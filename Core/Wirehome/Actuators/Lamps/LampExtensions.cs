using System;
using Wirehome.Contracts.Actuators;
using Wirehome.Contracts.Areas;

namespace Wirehome.Actuators.Lamps
{
    public static class LampExtensions
    {
        public static ILamp GetLamp(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<ILamp>($"{area.Id}.{id}");
        }
    }
}
