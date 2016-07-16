using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public class LDP433Accessor
    {
        private const int TIME_INTERVAL = 2;
        private const string SCHEDULER_NAME = "LDP433Updater";
        private readonly I2CHardwareBridge _i2CHardwareBridge;
        public event EventHandler<ReadLDP433MHzSignalCommandResponse> ValuesArrived;
        private byte _Pin = 0;

        public LDP433Accessor(I2CHardwareBridge i2CHardwareBridge, ISchedulerService sheduler)
        {
            if (i2CHardwareBridge == null) throw new ArgumentNullException(nameof(i2CHardwareBridge));
            if (sheduler == null) throw new ArgumentNullException(nameof(sheduler));

            _i2CHardwareBridge = i2CHardwareBridge;
            sheduler.RegisterSchedule(SCHEDULER_NAME, TimeSpan.FromSeconds(TIME_INTERVAL), FetchValues);
        }

        public void InitSensor(byte pin)
        {
            _Pin = pin;
        }

        private void FetchValues()
        {
            if (_Pin > 0)
            {
                var command = new ReadLDP433MHzSignalCommand().WithPin(_Pin);
                _i2CHardwareBridge.ExecuteCommand(command);

                if (command.Response.Codes.Count > 0)
                {
                    ValuesArrived?.Invoke(this, command.Response);
                }
            }
        }
    }
}
