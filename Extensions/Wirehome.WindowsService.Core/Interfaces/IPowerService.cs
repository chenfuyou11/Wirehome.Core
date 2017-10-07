namespace Wirehome.WindowsService.Services
{
    public interface IPowerService
    {
        void SetPowerMode(PowerState powerState);
    }
}