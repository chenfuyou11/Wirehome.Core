using System;
using Wirehome.Components;
using Wirehome.Contracts.Components;

namespace Wirehome.Conditions.Specialized
{
    public class TemperatureIsLowerThanCondition : Condition
    {
        public TemperatureIsLowerThanCondition(IComponent component, float? threshold)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            WithExpression(() =>
            {
                float? value;
                component.TryGetTemperature(out value);

                return value < threshold;
            });
        }
    }
}
