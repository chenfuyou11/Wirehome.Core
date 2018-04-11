using System;
using Wirehome.Components;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Sensors;
using Wirehome.Contracts.Triggers;
using Wirehome.Sensors.Triggers;

namespace Wirehome.Sensors.TemperatureSensors
{
    public static class TemperatureSensorExtensions
    {
        public static ITrigger GetTemperatureReachedTrigger(this IComponent component, float value, float delta)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return new SensorValueThresholdTrigger(component, s =>
            {
                float? v;
                s.TryGetTemperature(out v);
                return v;
            },
            SensorValueThresholdMode.Reached).WithTarget(value).WithDelta(delta);
        }

        public static ITrigger GetTemperatureUnderranTrigger(this IComponent component, float value, float delta)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return new SensorValueThresholdTrigger(component, s =>
            {
                float? v;
                s.TryGetTemperature(out v);
                return v;
            },
            SensorValueThresholdMode.Underran).WithTarget(value).WithDelta(delta);
        }
       
        public static ITemperatureSensor GetTemperatureSensor(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<ITemperatureSensor>($"{area.Id}.{id}");
        }
    }
}
