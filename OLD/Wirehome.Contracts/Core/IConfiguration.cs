using System.Threading.Tasks;

namespace Wirehome.Contracts.Core
{
    public interface IConfiguration
    {
        Task ApplyAsync();
    }
}
