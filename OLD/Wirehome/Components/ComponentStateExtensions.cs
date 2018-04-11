using System;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Logging;

namespace Wirehome.Components
{
    public static class ComponentStateExtensions
    {
        public static bool TryGetHumidity(this IComponent component, out float? value)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return TryGetStateValue<HumidityState, float?>(component, s => s.Value, out value);
        }

        public static bool TryGetTemperature(this IComponent component, out float? value)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return TryGetStateValue<TemperatureState, float?>(component, s => s.Value, out value);
        }

        public static bool TryGetPowerState(this IComponent component, out PowerStateValue? value)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return TryGetStateValue<PowerState, PowerStateValue?>(component, s => s.Value, out value);
        }

        public static bool TryGetMotionDetectionState(this IComponent component, out MotionDetectionStateValue value)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return TryGetStateValue<MotionDetectionState, MotionDetectionStateValue>(component, s => s.Value, out value);
        }

        public static bool TryGetButtonState(this IComponent component, out ButtonStateValue value)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return TryGetStateValue<ButtonState, ButtonStateValue>(component, s => s.Value, out value);
        }

        public static bool TryGetState(this IComponent component, out string value)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return TryGetStateValue<StateMachineFeatureState, string>(component, s => s.Value, out value);
        }
        
        public static bool TryGetStateValue<TState, TValue>(this IComponent component, Func<TState, TValue> valueResolver, out TValue value) where TState : IComponentFeatureState
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (valueResolver == null) throw new ArgumentNullException(nameof(valueResolver));

            value = default(TValue);

            var state = component.GetState();
            if (!state.Supports<TState>())
            {
                Log.Default.Warning($"Component '{component.Id}' does not support state '{typeof(TState).Name}'.");
                return false;
            }

            var temperatureState = component.GetState().Extract<TState>();
            value = valueResolver(temperatureState);

            return true;
        }
    }
}
