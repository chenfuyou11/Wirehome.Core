using System.Threading.Tasks;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Contracts.Components.Adapters
{
    public interface ILampAdapter
    {
        bool SupportsColor { get; }

        int ColorResolutionBits { get; }

        Task SetState(AdapterPowerState powerState, AdapterColor color, params IHardwareParameter[] hardwareParameters);
    }
}
