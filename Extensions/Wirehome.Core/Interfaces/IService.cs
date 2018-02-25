using System.Threading.Tasks;

namespace Wirehome.Core
{
    public interface IService
    {
        Task Initialize(); // TODO: Support multiple calls.
    }
}
