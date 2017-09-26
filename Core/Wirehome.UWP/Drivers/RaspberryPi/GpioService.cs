using System.Collections.Generic;
using Windows.Devices.Gpio;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Hardware.RaspberryPi;
using Wirehome.Contracts.Services;

namespace Wirehome.Hardware.Drivers.RaspberryPi
{
    public sealed class GpioService : ServiceBase, IGpioService
    {
        private readonly GpioController _gpioController = GpioController.GetDefault();

        private readonly Dictionary<int, GpioInputPort> _openInputPorts = new Dictionary<int, GpioInputPort>();
        private readonly Dictionary<int, GpioOutputPort> _openOutputPorts = new Dictionary<int, GpioOutputPort>();

        public IBinaryInput GetInput(int number, GpioPullMode pullMode, GpioInputMonitoringMode monitoringMode)
        {
            GpioInputPort port;
            lock (_openInputPorts)
            {
                if (_openInputPorts.TryGetValue(number, out port))
                {
                    return port;
                }

                var pin = _gpioController.OpenPin(number, GpioSharingMode.Exclusive);
                port = new GpioInputPort(pin, monitoringMode, pullMode);
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

                port = new GpioOutputPort(_gpioController.OpenPin(number, GpioSharingMode.Exclusive));
                _openOutputPorts.Add(number, port);
            }

            return port;
        }
    }
}
