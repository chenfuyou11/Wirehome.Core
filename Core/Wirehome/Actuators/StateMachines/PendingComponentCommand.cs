using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Commands;

namespace Wirehome.Actuators.StateMachines
{
    public class PendingComponentCommand
    {
        public IComponent Component { get; set; }

        public ICommand Command { get; set; }

        public void Execute()
        {
            Component?.ExecuteCommand(Command);
        }
    }
}
