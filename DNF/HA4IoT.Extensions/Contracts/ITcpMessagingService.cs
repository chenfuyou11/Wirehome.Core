using HA4IoT.Contracts.Messaging;
using HA4IoT.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Extensions.Messaging.Services
{
    public interface ITcpMessagingService : IService
    {
        void MessageHandler(Message<JObject> message);
    }
}