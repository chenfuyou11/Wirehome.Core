using System;

namespace Wirehome.Contracts.Components.Adapters
{
    public interface INumericSensorAdapter
    {
        event EventHandler<NumericSensorAdapterValueChangedEventArgs> ValueChanged;

        void Refresh();
    }
}
