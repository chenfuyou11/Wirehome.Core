using System;

namespace Wirehome.Contracts.Components.Adapters
{
    public class ButtonAdapterStateChangedEventArgs : EventArgs
    {
        public ButtonAdapterStateChangedEventArgs(AdapterButtonState state)
        {
            State = state;
        }

        public AdapterButtonState State { get; }
    }
}
