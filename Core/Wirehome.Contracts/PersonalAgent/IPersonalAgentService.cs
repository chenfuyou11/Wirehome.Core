using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.PersonalAgent
{
    public interface IPersonalAgentService : IService
    {
        string ProcessTextMessage(string message);
    }
}