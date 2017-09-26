using System;
using Wirehome.Contracts.Components.Adapters;

namespace Wirehome.Tests.Mockups.Adapters
{
    public class TestNumericSensorAdapter : INumericSensorAdapter
    {
        public event EventHandler<NumericSensorAdapterValueChangedEventArgs> ValueChanged;

        public void Refresh()
        {
        }

        public void UpdateValue(float? value)
        {
            ValueChanged?.Invoke(this, new NumericSensorAdapterValueChangedEventArgs(value));
        }
    }
}
