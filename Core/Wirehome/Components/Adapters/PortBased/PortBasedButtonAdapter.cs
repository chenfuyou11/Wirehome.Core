using System;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Components.Adapters.PortBased
{
    public class PortBasedButtonAdapter : IButtonAdapter
    {
        public PortBasedButtonAdapter(IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            input.StateChanged += ForwardState;
        }

        public event EventHandler<ButtonAdapterStateChangedEventArgs> StateChanged;
        
        private void ForwardState(object sender, BinaryStateChangedEventArgs e)
        {
            if (e.NewState == BinaryState.High)
            {
                StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Pressed));
            }
            else if (e.NewState == BinaryState.Low)
            {
                StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Released));
            }
        }
    }
}
