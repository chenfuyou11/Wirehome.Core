using System.Threading.Tasks;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Contracts.Components.Adapters
{
    public interface IRollerShutterAdapter
    {
        Task SetState(AdapterRollerShutterState state, params IHardwareParameter[] parameters);
    }
}
