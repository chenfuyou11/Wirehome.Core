using System;
using Wirehome.Contracts.Components.Adapters;

namespace Wirehome.Components.Adapters
{
    public class VirtualButtonAdapter : IButtonAdapter
    {
        public event EventHandler<ButtonAdapterStateChangedEventArgs> StateChanged;

        public void Press()
        {
            StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Pressed));
        }

        public void Release()
        {
            StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Released));
        }
    }
}
