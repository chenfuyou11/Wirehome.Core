using Wirehome.Contracts.Hardware;

namespace Wirehome.Contracts.Actuators
{
    public interface IStateMachineState
    {
        string Id { get; }

        void Activate(params IHardwareParameter[] parameters);

        void Deactivate(params IHardwareParameter[] parameters);
    }
}
