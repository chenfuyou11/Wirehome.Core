using System;
using Wirehome.Components;
using Wirehome.Contracts.Components;

namespace Wirehome.Conditions.Specialized
{
    public class TemperatureIsGreaterThanCondition : Condition
    {
        public TemperatureIsGreaterThanCondition(IComponent component, float? threshold)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            WithExpression(() =>
            {
                float? value;
                component.TryGetTemperature(out value);

                return value > threshold;
            });
        }
    }
}
