using HA4IoT.Contracts.Services;
using HA4IoT.Extensions.Messaging;

namespace HA4IoT.Extensions.Contracts
{
    public interface ISerialMessagingService : IService
    {
        void RegisterHandler(IBinaryMessage handler);
        void Close();
    }
}