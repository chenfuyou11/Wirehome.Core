using System;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Hardware.Gpio;
using Wirehome.Contracts.Logging;

namespace Wirehome.Hardware.Drivers.Gpio
{
    public sealed class GpioInputPort : IBinaryInput, IDisposable
    {
        private const int PollInterval = 15; // TODO: Set from constructor. Consider two classes with "IGpioMonitoringStrategy".

        private readonly INativeGpio _pin;
        private readonly INativeTimerSerice _nativeTimerSerice;

        // ReSharper disable once NotAccessedField.Local
        //private readonly Timer _timer;

        private BinaryState _latestState;

        public GpioInputPort(INativeGpio pin, INativeTimerSerice nativeTimerSerice, GpioInputMonitoringMode mode, GpioPullMode pullMode)
        {
            _pin = pin ?? throw new ArgumentNullException(nameof(pin));
            _nativeTimerSerice = nativeTimerSerice ?? throw new ArgumentNullException(nameof(nativeTimerSerice));
            if (pullMode == GpioPullMode.High)
            {
                _pin.SetDriveMode(NativeGpioPinDriveMode.InputPullUp);
            }
            else if (pullMode == GpioPullMode.Low)
            {
                _pin.SetDriveMode(NativeGpioPinDriveMode.InputPullDown);
            }
            else
            {
                _pin.SetDriveMode(NativeGpioPinDriveMode.Input);
            }
            
            if (mode == GpioInputMonitoringMode.Polling)
            {
                _nativeTimerSerice.CreatePeriodicTimer(PollState, TimeSpan.FromMilliseconds(PollInterval));
            }
            else if (mode == GpioInputMonitoringMode.Interrupt)
            {
                //_pin.DebounceTimeout = TimeSpan.FromTicks(DebounceTimeoutTicks); // TODO: Set from constructor.
                _pin.ValueChanged += HandleInterrupt;
            }

            _latestState = ReadAndConvert();
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public BinaryState Read()
        {
            return Update(ReadAndConvert());
        }

        public void Dispose()
        {
            _pin.ValueChanged -= HandleInterrupt;
            _pin?.Dispose();
        }

        private void HandleInterrupt()
        {
            var newState = ReadAndConvert();

            Log.Default.Verbose("Interrupt raised for GPIO" + _pin.PinNumber + ".");
            Update(newState);
        }

        private void PollState()
        {
            try
            {
                Update(ReadAndConvert());
            }
            catch (Exception exception)
            {
                Log.Default.Error(exception, $"Error while polling input state of GPIO{_pin.PinNumber}.");
            }
        }

        private BinaryState ReadAndConvert()
        {
            return _pin.Read() == NativeGpioPinValue.High ? BinaryState.High : BinaryState.Low;
        }

        private BinaryState Update(BinaryState newState)
        {
            var oldState = _latestState;

            if (oldState == newState)
            {
                return oldState;
            }

            _latestState = newState;

            try
            {
                StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, newState));
            }
            catch (Exception exception)
            {
                Log.Default.Error(exception, $"Error while reading input state of GPIO{_pin.PinNumber}.");
            }
            
            return newState;
        }
    }
}
