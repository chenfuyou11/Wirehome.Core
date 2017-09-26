using System;

namespace Wirehome.Contracts.Components.Adapters
{
    public class NumericSensorAdapterValueChangedEventArgs : EventArgs
    {
        public NumericSensorAdapterValueChangedEventArgs(float? value)
        {
            Value = value;
        }

        public float? Value { get; }
    }
}
