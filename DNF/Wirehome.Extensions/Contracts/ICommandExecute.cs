using HA4IoT.Contracts.Components.Commands;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Devices
{
    public interface ICommandExecute
    {
        Task ExecuteAsyncCommand<T>() where T : ICommand;
    }

}
