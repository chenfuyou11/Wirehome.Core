using Wirehome.Extensions.Messaging;
using Wirehome.Contracts.Core;
using Newtonsoft.Json.Linq;

namespace Wirehome.Extensions.Contracts
{
    public interface IBinaryMessage
    {
        bool CanDeserialize(byte messageType, byte messageSize);
        bool CanSerialize(string messageType);
        object Deserialize(IBinaryReader reader, byte? messageSize = default);
        byte[] Serialize(JObject message);
        MessageType Type();
    }
}