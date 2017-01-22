using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.I2CHardwareBridge;
using System.Diagnostics;

namespace HA4IoT.Hardware.CCTools
{
    public class CurrentPort : IBinaryInput
    {
        public const float LOW_STATE_VALUE = 0.20f;
        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;
        public byte Number { get; }
        public CurrentAccessor CurrentAccessor { get; }
        private bool InvertValue;
        private BinaryState _LastState = BinaryState.Low;

        public CurrentPort(byte number, CurrentAccessor currentAccessor)
        {
            if (currentAccessor == null) throw new ArgumentNullException(nameof(currentAccessor));

            Number = number;
            CurrentAccessor = currentAccessor;
            var sensor = CurrentAccessor.GetCurrentSensor(number);
            sensor.ValueChanged += Sensor_ValueChanged;
        }

        private void Sensor_ValueChanged(Object sender, Contracts.Sensors.NumericValueSensorEndpointValueChangedEventArgs e)
        {
            var oldState = _LastState;
            var newState = StateFromValue(e.NewValue);

            if (oldState == newState)
            {
                return;
            }

            StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, newState));
        }

        public BinaryState Read()
        {
            var value = CurrentAccessor.GetCurrent(Number);
            return StateFromValue(value);
        }

        private BinaryState StateFromValue(float value)
        {
            var state = BinaryState.Low;

            Debug.WriteLine($"CURRENT: {value} on port {Number}");

            if (value > LOW_STATE_VALUE)
            {
                state = BinaryState.High;
            }

            state = CoerceState(state);

            _LastState = state;

            return state;
        }

        IBinaryInput IBinaryInput.WithInvertedState(bool value)
        {
            InvertValue = value;
            return this;
        }

        private BinaryState CoerceState(BinaryState state)
        {
            if (InvertValue)
            {
                return state == BinaryState.High ? BinaryState.Low : BinaryState.High;
            }

            return state;
        }

       
    }
}