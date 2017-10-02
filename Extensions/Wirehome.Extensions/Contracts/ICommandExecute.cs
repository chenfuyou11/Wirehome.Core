using Wirehome.Contracts.Components.Commands;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Devices
{
    public interface ICommandExecute
    {
        Task ExecuteAsyncCommand<T>(T command = default) where T : ICommand;
    }

}
