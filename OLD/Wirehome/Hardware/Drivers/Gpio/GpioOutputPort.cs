using System;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Hardware.Drivers.Gpio
{
    public sealed class GpioOutputPort : IBinaryOutput, IDisposable
    {
        private readonly object _syncRoot = new object();
        private readonly INativeGpio _pin;

        private BinaryState _latestState;

        public GpioOutputPort(INativeGpio pin)
        {
            _pin = pin ?? throw new ArgumentNullException(nameof(pin));
            _pin.SetDriveMode(NativeGpioPinDriveMode.Output);
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;
        
        public BinaryState Read()
        {
            return _latestState;
        }

        public void Write(BinaryState state, WriteBinaryStateMode mode)
        {
            if (mode != WriteBinaryStateMode.Commit)
            {
                return;
            }

            BinaryState oldState;
            
            lock (_syncRoot)
            {
                if (state == _latestState)
                {
                    return;
                }

                _pin.Write(state == BinaryState.High ? NativeGpioPinValue.High : NativeGpioPinValue.Low);

                oldState = _latestState;
                _latestState = state;
            }

            StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, state));
        }

        public void Dispose()
        {
            _pin?.Dispose();
        }
    }
}
