using System.Threading.Tasks;

namespace Wirehome.Contracts.Services
{
    public interface IService
    {
        Task Initialize(); // TODO: Support multiple calls.
    }
}
