using System;
using Wirehome.Components;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Sensors;
using Wirehome.Contracts.Triggers;
using Wirehome.Sensors.Triggers;

namespace Wirehome.Sensors.HumiditySensors
{
    public static class HumiditySensorExtensions
    {
        public static ITrigger GetHumidityReachedTrigger(this IComponent component, float value, float delta = 5)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return new SensorValueThresholdTrigger(component, s =>
            {
                float? v;
                s.TryGetHumidity(out v);
                return v;
            },
            SensorValueThresholdMode.Reached).WithTarget(value).WithDelta(delta);
        }

        public static ITrigger GetHumidityUnderranTrigger(this IComponent component, float value, float delta = 5)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return new SensorValueThresholdTrigger(component, s =>
            {
                float? v;
                s.TryGetHumidity(out v);
                return v;
            },
            SensorValueThresholdMode.Underran).WithTarget(value).WithDelta(delta);
        }

        public static IHumiditySensor GetHumiditySensor(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IHumiditySensor>($"{area.Id}.{id}");
        }
    }
}
