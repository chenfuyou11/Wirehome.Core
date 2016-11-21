using HA4IoT.Contracts.Sensors;
using System;

namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public class CurrentSensor : INumericValueSensorEndpoint
    {
        private readonly int _id;
        private readonly CurrentAccessor _CurrentAccessor;
        private float _value;

        public CurrentSensor(int id, CurrentAccessor currentccessor)
        {
            if (currentccessor == null) throw new ArgumentNullException(nameof(currentccessor));

            _id = id;
            _CurrentAccessor = currentccessor;
            _CurrentAccessor.ValuesUpdated += (s, e) => UpdateValue();
        }

        public event EventHandler<NumericValueSensorEndpointValueChangedEventArgs> ValueChanged;

        private void UpdateValue()
        {
            _value = GetValueInternal((byte)_id, _CurrentAccessor);
            ValueChanged?.Invoke(this, new NumericValueSensorEndpointValueChangedEventArgs(_value));
        }

        protected float GetValueInternal(int id, CurrentAccessor currentAccessor)
        {
            return currentAccessor.GetCurrent((byte)id);
        }
    }
}
