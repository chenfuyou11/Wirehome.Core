using System.Collections.Generic;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Hardware.Gpio;
using Wirehome.Contracts.Services;

namespace Wirehome.Hardware.Drivers.Gpio
{
    public sealed class GpioService : ServiceBase, IGpioService
    {
        private readonly Dictionary<int, GpioInputPort> _openInputPorts = new Dictionary<int, GpioInputPort>();
        private readonly Dictionary<int, GpioOutputPort> _openOutputPorts = new Dictionary<int, GpioOutputPort>();
        private readonly INativeGpioController _nativeGpioController;
        private readonly INativeTimerSerice _nativeTimerSerice;

        public GpioService(INativeGpioController nativeGpioController, INativeTimerSerice nativeTimerSerice)
        {
            _nativeGpioController = nativeGpioController ?? throw new System.ArgumentNullException(nameof(nativeGpioController));
            _nativeTimerSerice = nativeTimerSerice ?? throw new System.ArgumentNullException(nameof(nativeTimerSerice));
        }
        
        public IBinaryInput GetInput(int number, GpioPullMode pullMode, GpioInputMonitoringMode monitoringMode)
        {
            GpioInputPort port;
            lock (_openInputPorts)
            {
                if (_openInputPorts.TryGetValue(number, out port))
                {
                    return port;
                }

                var pin = _nativeGpioController.OpenPin(number, NativeGpioSharingMode.Exclusive);
                port = new GpioInputPort(pin, _nativeTimerSerice, monitoringMode, pullMode);
                _openInputPorts.Add(number, port);
            }

            return port;
        }

        public IBinaryOutput GetOutput(int number)
        {
            GpioOutputPort port;
            lock (_openOutputPorts)
            {
                if (_openOutputPorts.TryGetValue(number, out port))
                {
                    return port;
                }

                port = new GpioOutputPort(_nativeGpioController.OpenPin(number, NativeGpioSharingMode.Exclusive));
                _openOutputPorts.Add(number, port);
            }

            return port;
        }
    }
}
