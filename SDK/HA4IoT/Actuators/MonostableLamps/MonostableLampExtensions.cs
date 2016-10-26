using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.Lamps
{
    public static class MonostablLampExtensions
    {
        public static IMonostableLamp GetMonostableLamp(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IMonostableLamp>(ComponentIdGenerator.Generate(area.Id, id));
        }
    }
}
