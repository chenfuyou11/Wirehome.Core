using System.Threading.Tasks;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.ExternalServices.Twitter
{
    public interface ITwitterClientService : IService
    {
        Task<bool> TryTweet(string message);
    }
}
