using System;

namespace Wirehome.Contracts.Components.Adapters
{
    public interface IMotionDetectorAdapter
    {
        event EventHandler<MotionDetectorAdapterStateChangedEventArgs> StateChanged;

        void Refresh();
    }
}
