using System.Threading.Tasks;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Contracts.Components.Adapters
{
    public interface IBinaryOutputAdapter
    {
        Task SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters);
    }
}
