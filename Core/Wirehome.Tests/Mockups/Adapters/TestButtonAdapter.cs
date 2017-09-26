using System;
using Wirehome.Contracts.Components.Adapters;

namespace Wirehome.Tests.Mockups.Adapters
{
    public class TestButtonAdapter : IButtonAdapter
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

        public void Touch()
        {
            Press();
            Release();
        }
    }
}
