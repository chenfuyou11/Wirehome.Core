using System;

namespace Wirehome.Contracts.Components.Adapters
{
    public interface IWindowAdapter
    {
        event EventHandler<WindowStateChangedEventArgs> StateChanged;

        void Refresh();
    }
}
