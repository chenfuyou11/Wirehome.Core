namespace Wirehome.Contracts.Components.States
{
    public abstract class SimpleValueState<T> : IComponentFeatureState
    {
        protected SimpleValueState(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
