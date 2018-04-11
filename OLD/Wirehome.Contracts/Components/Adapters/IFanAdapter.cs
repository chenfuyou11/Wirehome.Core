using Wirehome.Contracts.Hardware;

namespace Wirehome.Contracts.Components.Adapters
{
    public interface IFanAdapter
    {
        int MaxLevel { get; }

        void SetState(int level, params IHardwareParameter[] parameters);
    }
}
