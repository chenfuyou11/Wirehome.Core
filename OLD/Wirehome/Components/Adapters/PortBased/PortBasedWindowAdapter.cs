using System;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Components.Adapters.PortBased
{
    public class PortBasedWindowAdapter : IWindowAdapter
    {
        private readonly IBinaryInput _fullOpenReedSwitch;
        private readonly IBinaryInput _tildOpenReedSwitch;

        public PortBasedWindowAdapter(IBinaryInput fullOpenReedSwitch, IBinaryInput tildOpenReedSwitch = null)
        {
            _fullOpenReedSwitch = fullOpenReedSwitch ?? throw new ArgumentNullException(nameof(fullOpenReedSwitch));
            _tildOpenReedSwitch = tildOpenReedSwitch;

            if (_tildOpenReedSwitch != null)
            {
                _tildOpenReedSwitch.StateChanged += (s, e) => Refresh();
            }

            _fullOpenReedSwitch.StateChanged += (s, e) => Refresh();
        }

        public event EventHandler<WindowStateChangedEventArgs> StateChanged;

        public void Refresh()
        {
            var fullOpenReedSwitchState = _fullOpenReedSwitch.Read() == BinaryState.High
                ? AdapterSwitchState.Closed
                : AdapterSwitchState.Open;

            AdapterSwitchState? tildOpenReedSwitchState = null;
            if (_tildOpenReedSwitch != null)
            {
                tildOpenReedSwitchState = _tildOpenReedSwitch.Read() == BinaryState.High
                    ? AdapterSwitchState.Closed
                    : AdapterSwitchState.Open;
            }

            StateChanged?.Invoke(this, new WindowStateChangedEventArgs(fullOpenReedSwitchState, tildOpenReedSwitchState));
        }
    }
}
