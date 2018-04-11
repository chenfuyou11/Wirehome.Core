using Wirehome.Core.Interface.Messaging;

namespace Wirehome.Core.Services
{
    public interface ISerialMessagingService : IService
    {
        void RegisterBinaryMessage(IBinaryMessage message);
    }
}