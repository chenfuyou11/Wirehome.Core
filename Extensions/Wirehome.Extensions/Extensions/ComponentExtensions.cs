using System;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Logging;
using Wirehome.Extensions.Devices.States;

namespace Wirehome.Extensions.Extensions
{
    
    public static class ComponentStateExtensions
    {
        public static bool TryGetPowerState(this IComponent component, out PowerStateValue value)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return TryGetStateValue<PowerState, PowerStateValue>(component, s => s.Value, out value);
        }

        public static bool TryGetSurroundState(this IComponent component, out string surround)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return TryGetStateValue<SurroundState, string>(component, s => s.Value, out surround);
        }


        public static bool TryGetStateValue<TState, TValue>(this IComponent component, Func<TState, TValue> valueResolver, out TValue value) where TState : IComponentFeatureState
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (valueResolver == null) throw new ArgumentNullException(nameof(valueResolver));

            value = default;

            var state = component.GetState();
            if (!state.Supports<TState>())
            {
                Log.Default.Warning($"Component '{component.Id}' does not support state '{typeof(TState).Name}'.");
                return false;
            }
            value = valueResolver(component.GetState().Extract<TState>());

            return true;
        }
    }
}
