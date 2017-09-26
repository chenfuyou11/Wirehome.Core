using System;

namespace Wirehome.Contracts.Components.Adapters
{
    public class WindowStateChangedEventArgs : EventArgs
    {
        public WindowStateChangedEventArgs(AdapterSwitchState openReedSwitchState, AdapterSwitchState? tildReedSwitchState)
        {
            OpenReedSwitchState = openReedSwitchState;
            TildReedSwitchState = tildReedSwitchState;
        }

        public AdapterSwitchState OpenReedSwitchState { get; }

        public AdapterSwitchState? TildReedSwitchState { get; }
    }
}
