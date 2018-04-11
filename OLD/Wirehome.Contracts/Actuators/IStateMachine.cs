using Wirehome.Contracts.Components;

namespace Wirehome.Contracts.Actuators
{
    public interface IStateMachine : IComponent
    {
        string AlternativeStateId { get; set; }

        string ResetStateId { get; set; }

        bool SupportsState(string id);
    }
}
