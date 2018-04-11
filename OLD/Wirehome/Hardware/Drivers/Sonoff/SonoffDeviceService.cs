using System;
using System.Collections.Generic;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Hardware.DeviceMessaging;
using Wirehome.Contracts.Services;

namespace Wirehome.Hardware.Drivers.Sonoff
{
    public class SonoffDeviceService : ServiceBase
    {
        private readonly Dictionary<string, SonoffBinaryOutputAdapter> _adapters = new Dictionary<string, SonoffBinaryOutputAdapter>();
        private readonly IDeviceMessageBrokerService _deviceMessageBrokerService;

        public SonoffDeviceService(IDeviceMessageBrokerService deviceMessageBrokerService)
        {
            if (deviceMessageBrokerService == null) throw new ArgumentNullException(nameof(deviceMessageBrokerService));

            _deviceMessageBrokerService = deviceMessageBrokerService;
        }

        public IBinaryOutputAdapter GetAdapterForPow(string deviceName)
        {
            SonoffBinaryOutputAdapter adapter;
            if (!_adapters.TryGetValue(deviceName, out adapter))
            {
                adapter = new SonoffBinaryOutputAdapter($"cmnd/{deviceName}/power", _deviceMessageBrokerService);
                _adapters.Add(deviceName, adapter);
            }

            return adapter;
        }

        public IBinaryOutputAdapter GetAdapterForDualRelay1(string deviceName)
        {
            SonoffBinaryOutputAdapter adapter;
            if (!_adapters.TryGetValue(deviceName, out adapter))
            {
                adapter = new SonoffBinaryOutputAdapter($"cmnd/{deviceName}/power1", _deviceMessageBrokerService);
                _adapters.Add(deviceName, adapter);
            }

            return adapter;
        }

        public IBinaryOutputAdapter GetAdapterForDualRelay2(string deviceName)
        {
            SonoffBinaryOutputAdapter adapter;
            if (!_adapters.TryGetValue(deviceName, out adapter))
            {
                adapter = new SonoffBinaryOutputAdapter($"cmnd/{deviceName}/power2", _deviceMessageBrokerService);
                _adapters.Add(deviceName, adapter);
            }

            return adapter;
        }
    }
}
