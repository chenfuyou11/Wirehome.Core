using Wirehome.Contracts.Components.Commands;

namespace Wirehome.Components
{
    public interface ICommandExecutorAction
    {
        void Execute(ICommand command);
    }
}