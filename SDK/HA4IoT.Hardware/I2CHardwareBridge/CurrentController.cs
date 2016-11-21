using System;
using HA4IoT.Contracts.Hardware;
using System.Collections.Generic;
using HA4IoT.Hardware.I2CHardwareBridge;

namespace HA4IoT.Hardware.CCTools
{
    public class CurrentController : IBinaryInputController, IDevice
    {
        private CurrentAccessor _Accessor;

        private readonly object _syncRoot = new object();
        private readonly Dictionary<int, CurrentPort> _openPorts = new Dictionary<int, CurrentPort>();

        public DeviceId Id { get; } = new DeviceId("CurrentController");

        public CurrentController(CurrentAccessor accessor) 
        {
            _Accessor = accessor;
        }

        public IBinaryInput GetInput(int number)
        {
            return GetPort(number);
        }

        protected CurrentPort GetPort(int number)
        {
            lock (_syncRoot)
            {
                CurrentPort port;
                if (!_openPorts.TryGetValue(number, out port))
                {
                    port = new CurrentPort((byte)number, _Accessor);
                    _openPorts.Add(number, port);
                }

                return port;
            }
        }

        public IBinaryInput this[int pin] => GetInput((int)pin);
    }
}
