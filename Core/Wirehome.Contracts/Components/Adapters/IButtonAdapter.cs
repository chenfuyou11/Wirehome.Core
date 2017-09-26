using System;

namespace Wirehome.Contracts.Components.Adapters
{
    public interface IButtonAdapter
    {
        event EventHandler<ButtonAdapterStateChangedEventArgs> StateChanged;
    }
}
