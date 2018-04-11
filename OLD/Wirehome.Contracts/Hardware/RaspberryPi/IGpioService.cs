using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Hardware.Gpio
{
    public interface IGpioService : IService
    {
        IBinaryInput GetInput(int number, GpioPullMode pullMode, GpioInputMonitoringMode monitoringMode);

        IBinaryOutput GetOutput(int id);
    }
}