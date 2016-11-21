using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public class CurrentAccessor
    {
        private const int UPDATE_INTERVAL = 2;

        private readonly I2CHardwareBridge _i2CHardwareBridge;
        private readonly HashSet<byte> _openPins = new HashSet<byte>();
        private readonly Dictionary<byte, float> _currents = new Dictionary<byte, float>();

        public event EventHandler ValuesUpdated;

        public CurrentAccessor(I2CHardwareBridge i2CHardwareBridge, ISchedulerService schedulerService)
        {
            if (i2CHardwareBridge == null) throw new ArgumentNullException(nameof(i2CHardwareBridge));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            
            _i2CHardwareBridge = i2CHardwareBridge;
            schedulerService.RegisterSchedule("CurrentUpdater", TimeSpan.FromSeconds(UPDATE_INTERVAL), FetchValues);
        }
        
        public CurrentSensor GetCurrentSensor(byte pin)
        {
            _openPins.Add(pin);
            _currents[pin] = 0.0F;

            return new CurrentSensor(pin, this);
        }

        public float GetCurrent(byte pin)
        {
            return _currents[pin];
        }
        
        private void FetchValues()
        {
            foreach (var openPin in _openPins)
            {
                FetchValues(openPin);
            }
            
            ValuesUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void FetchValues(byte pin)
        {
            var command = new ReadCurrentSensorCommand().WithPin(pin);
            _i2CHardwareBridge.ExecuteCommand(command);

            if (command.Response != null && command.Response.Succeeded)
            {
                _currents[pin] = command.Response.Current;
            }
        }
    }
}
