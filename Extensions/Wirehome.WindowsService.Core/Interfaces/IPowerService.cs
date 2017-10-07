using Wirehome.Extensions.Core;

namespace Wirehome.WindowsService.Services
{
    public interface IPowerService
    {
        void SetPowerMode(ComputerPowerState powerState);
    }
}